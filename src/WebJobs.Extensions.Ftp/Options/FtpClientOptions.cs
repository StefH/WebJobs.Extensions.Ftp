using System.ComponentModel.DataAnnotations;

namespace WebJobs.Extensions.Ftp.Options;

public class FtpClientOptions
{
    [Required]
    public string Host { get; set; } = null!;

    [Required]
    [Range(0, ushort.MaxValue)]
    public int Port { get; set; } = 21;

    public string? Username { get; set; }

    public string? Password { get; set; }

    /// <summary>
    /// HasCredentials
    /// </summary>
    /// <returns>True if the connection string has credentials, otherwise false</returns>
    public bool HasCredentials => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
}