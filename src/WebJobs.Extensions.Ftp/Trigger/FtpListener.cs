using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
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
    private readonly Type _triggerValueType;
    private readonly ITriggeredFunctionExecutor _executor;
    private readonly FtpTriggerContext _context;

    private DateTime _lastRunningTime = DateTime.MaxValue;

    public FtpListener(Type triggerValueType, ITriggeredFunctionExecutor executor, FtpTriggerContext context)
    {
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

        try
        {
            await RunRecurringTaskAsync(
                GetListingAndGetFilesAsync,
                _context.FtpTriggerAttribute.PollingIntervalInSeconds,
                cancellationToken
            );
        }
        catch (TaskCanceledException)
        {
            // Ignore TaskCanceledException
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
            foreach (var item in filteredListItems)
            {
                object value;
                if (_triggerValueType == typeof(FtpFile))
                {
                    value = await HandleFtpFileAsync(item, cancellationToken);
                }
                else
                {
                    value = await HandleFtpStreamAsync(item, cancellationToken);
                }

                var triggerData = new TriggeredFunctionData
                {
                    TriggerValue = value
                };
                await _executor.TryExecuteAsync(triggerData, cancellationToken);
            }
        }

        if (Constants.BatchTypes.Contains(_triggerValueType))
        {
            foreach (var items in filteredListItems.Page(_context.FtpTriggerAttribute.BatchSize))
            {
                object value;
                if (_triggerValueType == typeof(FtpFile[]))
                {
                    var list = new List<FtpFile>();
                    foreach (var item in items)
                    {
                        list.Add(await HandleFtpFileAsync(item, cancellationToken));
                    }

                    value = list.ToArray();
                }
                else
                {
                    var list = new List<FtpStream>();
                    foreach (var item in items)
                    {
                        list.Add(await HandleFtpStreamAsync(item, cancellationToken));
                    }

                    value = list.ToArray();
                }

                var triggerData = new TriggeredFunctionData
                {
                    TriggerValue = value
                };
                await _executor.TryExecuteAsync(triggerData, cancellationToken);
            }
        }
    }

    private async Task<FtpStream> HandleFtpStreamAsync(FtpListItem item, CancellationToken cancellationToken)
    {
        var ftpStream = TinyMapperUtils.Instance.MapToFtpStream(item);

        if (_context.FtpTriggerAttribute.IncludeContent)
        {
            ftpStream.Stream = await _context.Client.OpenReadAsync(item.FullName, token: cancellationToken);
        }

        return ftpStream;
    }

    private async Task<FtpFile> HandleFtpFileAsync(FtpListItem item, CancellationToken cancellationToken)
    {
        var ftpFile = TinyMapperUtils.Instance.MapToFtpFileItem(item);

        if (_context.FtpTriggerAttribute.IncludeContent)
        {
            using var stream = new MemoryStream();
            await _context.Client.DownloadAsync(stream, item.FullName, token: cancellationToken);
            ftpFile.Content = stream.ToArray();
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

    private Task RunRecurringTaskAsync(Func<CancellationToken, Task> action, int seconds, CancellationToken token)
    {
        return Task.Run(async () =>
        {
            if (_context.FtpTriggerAttribute.RunOnStartup)
            {
                _lastRunningTime = DateTime.UtcNow;
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds), token);
            }

            while (!token.IsCancellationRequested)
            {
                await action(token);

                _lastRunningTime = DateTime.UtcNow;

                await Task.Delay(TimeSpan.FromSeconds(seconds), token);
            }
        }, token);
    }
}