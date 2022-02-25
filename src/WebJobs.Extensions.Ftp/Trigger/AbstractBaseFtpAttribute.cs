using System;

namespace WebJobs.Extensions.Ftp.Trigger;

public abstract class AbstractBaseFtpAttribute : Attribute
{
    public string Connection { get; set; } = null!;

    /// <summary>
    /// Helper method to get ConnectionString from environment variable. If that fails, use the ConnectionString as-is.
    /// </summary>
    internal string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(Connection) ?? Connection;
    }
}