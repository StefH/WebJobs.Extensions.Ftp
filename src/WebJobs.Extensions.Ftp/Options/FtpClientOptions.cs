using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebJobs.Extensions.Ftp.Options;

public class FtpClientOptions
{
    /// <summary>
    /// Server regex pattern
    /// </summary>
    private const string ServerPattern = @"(?<host>[^:]*):?(?<port>\d*)";

    /// <summary>
    /// Rest of the connection string regex
    /// </summary>
    private const string Pattern = @"^(?<scheme>(ftp|ftps))://" + @"((?<username>[^:@/]+)(:(?<password>[^:@/]*))?@)?" + ServerPattern + "$";

    /// <summary>
    /// Default port
    /// </summary>
    private const int DefaultPort = 21;

    [Required]
    public string Host { get; set; } = null!;

    [Required]
    [Range(0, ushort.MaxValue)]
    public int Port { get; set; } = DefaultPort;

    public string? Username { get; set; }

    public string? Password { get; set; }

    /// <summary>
    /// HasCredentials
    /// </summary>
    /// <returns>True if the connection string has credentials, otherwise false</returns>
    public bool HasCredentials => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    /// <summary>
    /// Parse connection string in the form of: ftp(s)://{username}:{password}@{host}:{port} and return FtpClientOptions instance.
    /// </summary>
    /// <param name="connectionString">Connection string</param>
    /// <returns>Returns the ConnectionParams instance with parsed values</returns>
    public static FtpClientOptions Parse(string connectionString)
    {
        var match = Regex.Match(connectionString, Pattern);
        if (!match.Success)
        {
            throw new ArgumentException($"The ConnectionString '{connectionString}' cannot be parsed to a valid FTP connection string.", nameof(connectionString));
        }

        return new FtpClientOptions
        {
            Host = match.Groups["host"].Value,
            Port = int.TryParse(match.Groups["port"].Value, out var port) ? port : DefaultPort,
            Username = GetOptionalStringValue(match.Groups["username"]),
            Password = GetOptionalStringValue(match.Groups["password"])
        };
    }

    private static string? GetOptionalStringValue(Group group)
    {
        return group.Success ? group.Value : null;
    }
}