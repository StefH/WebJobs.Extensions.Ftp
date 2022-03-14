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
    /// <param name="connect">Automatically connect</param>
    /// <returns>Returns IFtpClient instance</returns>
    public IFtpClient CreateClient(bool connect = false);

    /// <summary>
    /// Create Ftp Client from connection string
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="connect">Automatically connect</param>
    /// <returns>Returns IFtpClient instance</returns>
    public IFtpClient CreateClient(string name, bool connect = false);
}