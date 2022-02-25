using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Stef.Validation;
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
            .OrderBy(li => li.RawModified);

        if (Constants.SingleTypes.Contains(_triggerValueType))
        {
            foreach (var item in filteredListItems)
            {
                await HandleFtpListItemAsync(item, cancellationToken);
            }
        }

        if (Constants.BatchTypes.Contains(_triggerValueType))
        {

        }
    }

    private async Task HandleFtpListItemAsync(FtpListItem item, CancellationToken cancellationToken)
    {
        object triggerValue;
        if (_triggerValueType == typeof(FtpFile))
        {
            var ftpFile = TinyMapperUtils.Instance.MapToFtpFileItem(item);

            if (_context.FtpTriggerAttribute.IncludeContent)
            {
                using var stream = new MemoryStream();
                await _context.Client.DownloadAsync(stream, item.FullName, token: cancellationToken);
                ftpFile.Content = stream.ToArray();
            }

            triggerValue = ftpFile;
        }
        else if (_triggerValueType == typeof(FtpStream))
        {
            var ftpStream = TinyMapperUtils.Instance.MapToFtpStream(item);

            if (_context.FtpTriggerAttribute.IncludeContent)
            {
                ftpStream.Stream = await _context.Client.OpenReadAsync(item.FullName, token: cancellationToken);
            }

            triggerValue = ftpStream;
        }
        else
        {
            throw new NotSupportedException($"Invalid trigger value type. Only {string.Join(",", Constants.SupportedTypes.Select(t => t.Name))} are supported.");
        }

        var triggerData = new TriggeredFunctionData
        {
            TriggerValue = triggerValue
        };
        await _executor.TryExecuteAsync(triggerData, cancellationToken);
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