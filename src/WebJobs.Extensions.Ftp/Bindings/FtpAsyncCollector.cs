using System;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Models;

namespace WebJobs.Extensions.Ftp.Bindings;

/// <summary>
/// Async Collector class. Responsible for publishing to a FTP Server
/// </summary>
/// <typeparam name="T">Data Type of value</typeparam>
internal class FtpAsyncCollector<T> : IAsyncCollector<T>
{
    private readonly ILogger _logger;

    /// <summary>
    /// FtpBindingContext instance
    /// </summary>
    private readonly FtpBindingContext _context;

    public FtpAsyncCollector(ILogger logger, FtpBindingContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// UploadAsync to FTP
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="cancellationToken">A Cancellation Token</param>
    /// <returns>A Task that completes when the item is added.</returns>
    public Task AddAsync(T item, CancellationToken cancellationToken = default)
    {
        return item switch
        {
            FtpFile ftpFile => AddFtpFileAsync(ftpFile, cancellationToken),
            FtpStream ftpStream => AddFtpStreamAsync(ftpStream, cancellationToken),

            _ => throw new InvalidCastException($"Item of type '{item?.GetType()}' is not supported.")
        };
    }

    private async Task AddFtpFileAsync(FtpFile ftpFile, CancellationToken cancellationToken)
    {
        await CheckIfConnectedAsync(cancellationToken);

        var remotePath = $"{_context.FtpAttribute.Folder}/{ftpFile.Name}";
        var status = await _context.Client.UploadAsync(ftpFile.Content, remotePath, token: cancellationToken);
        if (status != FtpStatus.Success)
        {
            _logger.LogWarning("UploadAsync({type}) for file '{path}' returned {status}.", typeof(FtpFile), remotePath, status);
        }
    }

    private async Task AddFtpStreamAsync(FtpStream ftpStream, CancellationToken cancellationToken)
    {
        await CheckIfConnectedAsync(cancellationToken);

        var remotePath = $"{_context.FtpAttribute.Folder}/{ftpStream.Name}";
        var status = await _context.Client.UploadAsync(ftpStream.Stream, remotePath, token: cancellationToken);
        if (status != FtpStatus.Success)
        {
            _logger.LogWarning("UploadAsync({type}) for file '{path}' returned {status}.", typeof(FtpStream), remotePath, status);
        }
    }

    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private async Task CheckIfConnectedAsync(CancellationToken cancellationToken)
    {
        if (!_context.Client.IsConnected)
        {
            await _context.Client.ConnectAsync(cancellationToken);
        }
    }
}