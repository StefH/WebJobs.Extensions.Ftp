using System.Net;
using FluentFTP;
using Stef.Validation;
using WebJobs.Extensions.Ftp.Utils;

namespace WebJobs.Extensions.Ftp.Factories;

internal class FtpClientFactory : IFtpClientFactory
{
    private readonly NetworkCredential _anonymousCredentials = new("anonymous", "anonymous");

    /// <inheritdoc cref="IFtpClientFactory.CreateFtpClient"/>
    public IFtpClient CreateFtpClient(string connectionString)
    {
        Guard.NotNullOrEmpty(connectionString);

        var parsed = FtpUrlParser.Parse(connectionString);

        return parsed.HasCredentials ?
            new FtpClient(parsed.Host, parsed.Port, parsed.Username, parsed.Password) :
            new FtpClient(parsed.Host, parsed.Port, _anonymousCredentials);
    }
}