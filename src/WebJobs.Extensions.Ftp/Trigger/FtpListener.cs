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
using Stef.Validation;
using WebJobs.Extensions.Ftp.Extensions;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Utils;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// The FtpListener class.
/// Implements the <c>IListener</c> interface. Contains the code to connect to a Ftp server.
/// </summary>
internal class FtpListener : IListener
{
    private readonly ILogger _logger;
    private readonly Type _triggerValueType;
    private readonly ITriggeredFunctionExecutor _executor;
    private readonly FtpTriggerContext _context;

    private DateTime _lastRunningTime = DateTime.MaxValue;
    private TimeSpan _pollingInterval = TimeSpan.MaxValue;

    public FtpListener(ILogger logger, Type triggerValueType, ITriggeredFunctionExecutor executor, FtpTriggerContext context)
    {
        _logger = Guard.NotNull(logger);
        _triggerValueType = Guard.NotNull(triggerValueType);
        _executor = Guard.NotNull(executor);
        _context = Guard.NotNull(context);
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
        await _context.Client.ConnectAsync(cancellationToken);

        _pollingInterval = PollingIntervalParser.Parse(_context.FtpTriggerAttribute.PollingInterval);

        try
        {
            await RunRecurringTaskAsync(GetListingAndGetFilesAsync, cancellationToken);
        }
        catch (Exception ex)
        {
            // Ignore any Exception and only log
            _logger.LogError(ex, ex.Message);
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
                await TryExecuteAsync(cancellationToken, file);
            }
            else
            {
                var stream = await HandleFtpStreamAsync(item, cancellationToken);
                await TryExecuteAsync(cancellationToken, stream);
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

                await TryExecuteAsync(cancellationToken, files.ToArray());
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

                await TryExecuteAsync(cancellationToken, streams.ToArray());
            }
        }
    }

    private async Task TryExecuteAsync<T>(CancellationToken cancellationToken, params T?[]? items)
        where T : class
    {
        if (items == null || items.Length == 0)
        {
            // Do not trigger for null file or empty list
            return;
        }

        var triggerData = new TriggeredFunctionData
        {
            TriggerValue = items.Length == 1 ? items[0] : items
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
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _context.Client.DisconnectAsync(cancellationToken);
    }

    private async Task<FtpListItem[]> GetListingAsync(CancellationToken cancellationToken)
    {
        var listOption = _context.FtpTriggerAttribute.Recursive ? FtpListOption.Recursive : FtpListOption.Auto;

        return await _context.Client.GetListingAsync(_context.FtpTriggerAttribute.Folder, listOption, cancellationToken);
    }

    private Task RunRecurringTaskAsync(Func<CancellationToken, Task> action, CancellationToken token)
    {
        return Task.Run(async () =>
        {
            if (_context.FtpTriggerAttribute.RunOnStartup)
            {
                _lastRunningTime = DateTime.UtcNow;
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(_pollingInterval.Seconds), token);
            }

            while (!token.IsCancellationRequested)
            {
                await action(token);

                _lastRunningTime = DateTime.UtcNow;

                await Task.Delay(TimeSpan.FromSeconds(_pollingInterval.Seconds), token);
            }
        }, token);

    }
}