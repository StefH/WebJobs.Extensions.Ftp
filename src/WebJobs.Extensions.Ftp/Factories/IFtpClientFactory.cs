using FluentFTP;

namespace WebJobs.Extensions.Ftp.Factories;

/// <summary>
/// Ftp Service factory. Create IFtpClient.
/// </summary>
internal interface IFtpClientFactory
{
    /// <summary>
    /// Create Ftp Client from connection string
    /// </summary>
    /// <param name="connectionString">Ftp Connection string</param>
    /// <returns>Returns FtpClient instance</returns>
    public IFtpClient CreateFtpClient(string connectionString);
}