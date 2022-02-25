using System;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Stef.Validation;
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
        _logger = Guard.NotNull(logger);
        _context = Guard.NotNull(context);
    }

    /// <summary>
    /// UploadAsync to FTP
    /// </summary>
    /// <param name="message">Message to be published</param>
    /// <param name="cancellationToken">A Cancellation Token</param>
    /// <returns>A Task that completes when the message us published</returns>
    public Task AddAsync(T message, CancellationToken cancellationToken = default)
    {
        return message switch
        {
            FtpFile ftpFile => AddFtpFileAsync(ftpFile, cancellationToken),
            FtpStream ftpStream => AddFtpStreamAsync(ftpStream, cancellationToken),

            _ => throw new InvalidCastException($"Message of type '{message?.GetType()}' is not supported.")
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