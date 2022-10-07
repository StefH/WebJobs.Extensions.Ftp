using System;
using System.Configuration;

namespace WebJobs.Extensions.Ftp.Trigger;

public abstract class AbstractBaseFtpAttribute : Attribute
{
    /// <summary>
    /// The Connection represents the FTP connection string.
    /// </summary>
    public string Connection { get; set; } = null!;

    protected AbstractBaseFtpAttribute()
    {
    }

    protected AbstractBaseFtpAttribute(string connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Helper method to get ConnectionString from environment variable.
    /// If that fails, use ConfigurationManager.ConnectionStrings (WebJobs)
    /// If that fails, use ConfigurationManager.AppSettings (WebJobs)
    /// Else use the ConnectionString as-is.
    /// </summary>
    internal string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(Connection) ??
               ConfigurationManager.ConnectionStrings[Connection]?.ConnectionString ??
               ConfigurationManager.AppSettings[Connection] ??
               Connection;
    }
}