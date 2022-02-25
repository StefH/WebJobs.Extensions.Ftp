using System.Text.RegularExpressions;
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
        var matches = Regex.Match(connectionString, Pattern);
        return new FtpConnectionParameters
        {
            Host = matches.Groups["host"].ToString(),
            Port = int.TryParse(matches.Groups["port"].ToString(), out var p) ? p : DefaultPort,
            Username = matches.Groups["username"].ToString(),
            Password = matches.Groups["password"].ToString()
        };
    }
}