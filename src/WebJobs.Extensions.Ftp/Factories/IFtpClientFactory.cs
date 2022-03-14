using FluentFTP;

namespace WebJobs.Extensions.Ftp.Factories;

/// <summary>
/// Ftp Client factory to create IFtpClient.
/// </summary>
public interface IFtpClientFactory
{
    /// <summary>
    /// Create Ftp Client.
    /// </summary>
    /// <returns>Returns IFtpClient instance</returns>
    public IFtpClient CreateClient();

    /// <summary>
    /// Create Ftp Client from connection string
    /// </summary>
    /// <param name="name">name</param>
    /// <returns>Returns IFtpClient instance</returns>
    public IFtpClient CreateClient(string name);
}