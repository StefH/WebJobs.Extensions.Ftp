using System;

namespace WebJobs.Extensions.Ftp.Trigger;

public abstract class AbstractBaseFtpAttribute : Attribute
{
    public string Connection { get; set; } = null!;

    /// <summary>
    /// Call Connect() on the bound IFtpClient
    ///
    /// Default value is <c>false</c>.
    /// </summary>
    public bool AutoConnectFtpClient { get; set; }

    protected AbstractBaseFtpAttribute()
    {
    }

    protected AbstractBaseFtpAttribute(string connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Helper method to get ConnectionString from environment variable.
    /// If that fails, use the ConnectionString as-is.
    /// Else throw.
    /// </summary>
    internal string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(Connection) ?? Connection;
    }
}