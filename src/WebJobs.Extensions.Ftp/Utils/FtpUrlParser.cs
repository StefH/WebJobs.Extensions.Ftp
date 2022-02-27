using System;
using System.Text.RegularExpressions;
using WebJobs.Extensions.Ftp.Extensions;
using WebJobs.Extensions.Ftp.Models.Internal;

namespace WebJobs.Extensions.Ftp.Utils;

/// <summary>
/// FtpUrlParser, a utility class to parse the Ftp Connection String.
/// The connection string is in the form of: ftp://<username>:<password>@<host>:<port>
/// </summary>
/// <example>
/// <code>
/// ftp://user:pwd@localhost:4222
/// </code>
/// </example>
internal static class FtpUrlParser
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

    /// <summary>
    /// Parse connection string and return ConnectionParams instance
    /// </summary>
    /// <param name="connectionString">Connection string</param>
    /// <returns>Returns the ConnectionParams instance with parsed values</returns>
    public static FtpConnectionParameters Parse(string connectionString)
    {
        var match = Regex.Match(connectionString, Pattern);
        if (!match.Success)
        {
            throw new ArgumentException($"The ConnectionString '{connectionString}' cannot be parsed to a valid FTP connection string.", nameof(connectionString));
        }

        return new FtpConnectionParameters
        {
            Host = match.Groups["host"].Value,
            Port = int.TryParse(match.Groups["port"].Value, out var p) ? p : DefaultPort,
            Username = match.Groups["username"].GetOptionalStringValue(),
            Password = match.Groups["password"].GetOptionalStringValue()
        };
    }
}