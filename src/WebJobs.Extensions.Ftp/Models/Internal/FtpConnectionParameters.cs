namespace WebJobs.Extensions.Ftp.Models.Internal;

internal class FtpConnectionParameters
{
    public string Host { get; internal set; } = null!;

    public int Port { get; internal set; }

    public string Username { get; internal set; } = null!;

    public string Password { get; internal set; } = null!;

    /// <summary>
    /// HasCredentials
    /// </summary>
    /// <returns>True if the connection string has credentials, otherwise false</returns>
    public bool HasCredentials => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
}