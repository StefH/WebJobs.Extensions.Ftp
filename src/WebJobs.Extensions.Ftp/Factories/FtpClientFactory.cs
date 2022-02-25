using FluentFTP;
using Stef.Validation;
using WebJobs.Extensions.Ftp.Utils;

namespace WebJobs.Extensions.Ftp.Factories;

internal class FtpClientFactory : IFtpClientFactory
{
    /// <inheritdoc cref="IFtpClientFactory.CreateFtpClient"/>
    public IFtpClient CreateFtpClient(string connectionString)
    {
        Guard.NotNullOrEmpty(connectionString);

        var parsed = FtpUrlParser.Parse(connectionString);
        return new FtpClient(parsed.Host, parsed.Port, parsed.Username, parsed.Password);
    }
}