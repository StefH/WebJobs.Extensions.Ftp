using System;
using System.Collections.Concurrent;
using FluentFTP;
using Microsoft.Extensions.Options;
using WebJobs.Extensions.Ftp.Options;

namespace WebJobs.Extensions.Ftp.Factories;

internal class FtpClientFactory : IFtpClientFactory
{
    private readonly ConcurrentDictionary<string, IFtpClient> _instances = new();

    private readonly IOptionsFactory<FtpClientOptions> _options;

    public FtpClientFactory(IOptionsFactory<FtpClientOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public IFtpClient CreateClient()
    {
        return CreateClient(Constants.DefaultFtpClientName);
    }

    /// <inheritdoc />
    public IFtpClient CreateClient(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Argument is null or empty", nameof(name));
        }

        return _instances.GetOrAdd(name, _ => FtpClientHelper.CreateFtpClient(_options.Create(name)));
    }
}