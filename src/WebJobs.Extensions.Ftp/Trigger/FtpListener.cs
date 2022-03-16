using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Extensions;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Utils;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// The FtpListener class.
/// Implements the <c>IListener</c> interface. Contains the code to connect to a Ftp server.
/// </summary>
internal sealed class FtpListener : IListener
{
    private readonly ILogger _logger;
    private readonly Type _triggerValueType;
    private readonly ITriggeredFunctionExecutor _executor;
    private readonly FtpTriggerContext _context;

    private DateTime _lastRunningTime;
    private TimeSpan _pollingInterval;

    public FtpListener(ILogger logger, Type triggerValueType, ITriggeredFunctionExecutor executor, FtpTriggerContext context)
    {
        _logger = logger;
        _triggerValueType = triggerValueType;
        _executor = executor;
        _context = context;
    }

    public void Cancel()
    {
        _context.Client.Disconnect();
    }

    public void Dispose()
    {
        _context.Client.Dispose();
    }

    /// <summary>
    /// Start the listener asynchronously. Subscribe to Ftp and wait for files. When a file is added, execute the function
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A Task returned from RecurringTask method</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _pollingInterval = PollingIntervalParser.Parse(_context.FtpTriggerAttribute.PollingInterval);

        try
        {
            await _context.Client.ConnectAsync(cancellationToken);

            await RunRecurringTaskAsync(GetListingAndGetFilesAsync, cancellationToken);
        }
        catch (FtpAuthenticationException ftpEx)
        {
            _logger.LogError(ftpEx, "User '{user}' is unable to connect to '{host}'. Please check credentials.", _context.Client.Credentials.UserName, _context.Client.Host);
            throw;
        }
        catch (Exception ex)
        {
            // Ignore any Exception and only log the exception
            _logger.LogWarning(ex, ex.Message);
        }
    }

    private async Task GetListingAndGetFilesAsync(CancellationToken cancellationToken)
    {
        var listItems = await GetListingAsync(cancellationToken);

        var filteredListItems = listItems
            .Where(li => li.Type == FtpFileSystemObjectType.File)
            .Where(li => li.RawModified > _lastRunningTime)
            .OrderBy(li => li.RawModified)
            .ToArray();

        if (Constants.SingleTypes.Contains(_triggerValueType))
        {
            await ProcessSingleAsync(cancellationToken, filteredListItems);
        }

        if (Constants.BatchTypes.Contains(_triggerValueType))
        {
            await ProcessBatchAsync(cancellationToken, filteredListItems);
        }
    }

    private async Task ProcessSingleAsync(CancellationToken cancellationToken, FtpListItem[] filteredListItems)
    {
        foreach (var item in filteredListItems)
        {
            if (_triggerValueType == typeof(FtpFile))
            {
                var file = await HandleFtpFileAsync(item, cancellationToken);
                if (file != null)
                {
                    await TryExecuteAsync(cancellationToken, FileType.Single, file);
                }
            }
            else
            {
                var stream = await HandleFtpStreamAsync(item, cancellationToken);
                if (stream != null)
                {
                    await TryExecuteAsync(cancellationToken, FileType.Single, stream);
                }
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken, FtpListItem[] filteredListItems)
    {
        foreach (var items in filteredListItems.GetBatches(_context.FtpTriggerAttribute.BatchSize))
        {
            if (_triggerValueType == typeof(FtpFile[]))
            {
                var files = new List<FtpFile>();
                foreach (var item in items)
                {
                    var file = await HandleFtpFileAsync(item, cancellationToken);
                    if (file != null)
                    {
                        files.Add(file);
                    }
                }

                await TryExecuteAsync(cancellationToken, FileType.Batch, files.ToArray());
            }
            else
            {
                var streams = new List<FtpStream>();
                foreach (var item in items)
                {
                    var stream = await HandleFtpStreamAsync(item, cancellationToken);
                    if (stream != null)
                    {
                        streams.Add(stream);
                    }
                }

                await TryExecuteAsync(cancellationToken, FileType.Batch, streams.ToArray());
            }
        }
    }

    private async Task TryExecuteAsync<T>(CancellationToken cancellationToken, FileType type, params T[] items)
    {
        if (items.Length == 0)
        {
            // Do not trigger when no items.
            return;
        }

        var triggerData = new TriggeredFunctionData
        {
            TriggerValue = type == FileType.Single ? items[0] : items
        };
        await _executor.TryExecuteAsync(triggerData, cancellationToken);
    }

    private async Task<FtpStream?> HandleFtpStreamAsync(FtpListItem item, CancellationToken cancellationToken)
    {
        var ftpStream = TinyMapperUtils.Instance.MapToFtpStream(item);

        if (_context.FtpTriggerAttribute.IncludeContent)
        {
            try
            {
                ftpStream.Stream = await _context.Client.OpenReadAsync(item.FullName, token: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to open the file '{fullName}' for reading, this {type} will not be included in the trigger value.", item.FullName, typeof(FtpStream));
                return null;
            }
        }

        return ftpStream;
    }

    private async Task<FtpFile?> HandleFtpFileAsync(FtpListItem item, CancellationToken cancellationToken)
    {
        var ftpFile = TinyMapperUtils.Instance.MapToFtpFileItem(item);

        if (_context.FtpTriggerAttribute.IncludeContent)
        {
            try
            {
                await using var stream = new MemoryStream();
                await _context.Client.DownloadAsync(stream, item.FullName, token: cancellationToken);
                ftpFile.Content = stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to download file '{fullName}', this {type} will not be included in the trigger value.", item.FullName, typeof(FtpFile));
                return null;
            }
        }

        return ftpFile;
    }

    /// <summary>
    /// Stop current listening operation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.Client.DisconnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Ignore any Exception and only log the exception
            _logger.LogWarning(ex, "Error during stopping {client}.", nameof(FtpClient));
        }
    }

    private async Task<FtpListItem[]> GetListingAsync(CancellationToken cancellationToken)
    {
        try
        {
            var listOption = FtpListOption.Auto;
            if (_context.FtpTriggerAttribute.LoadModifyDateUsingMDTM)
            {
                listOption |= FtpListOption.Modify;
            }

            if (_context.FtpTriggerAttribute.Recursive)
            {
                listOption |= FtpListOption.Recursive;
            }

            return await _context.Client.GetListingAsync(_context.FtpTriggerAttribute.Folder, listOption, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to get listing for folder '{folder}'. No items will be included in the trigger value.", _context.FtpTriggerAttribute.Folder ?? "/");
            return Array.Empty<FtpListItem>();
        }
    }

    private Task RunRecurringTaskAsync(Func<CancellationToken, Task> action, CancellationToken token)
    {
        return Task.Run(async () =>
        {
            _lastRunningTime = DateTime.UtcNow;

            if (!_context.FtpTriggerAttribute.RunOnStartup)
            {
                await Task.Delay(_pollingInterval, token);
            }

            while (!token.IsCancellationRequested)
            {
                await action(token);

                _lastRunningTime = DateTime.UtcNow;

                await Task.Delay(_pollingInterval, token);
            }
        }, token);
    }
}