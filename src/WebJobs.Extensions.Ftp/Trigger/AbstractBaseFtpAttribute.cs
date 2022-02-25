using System;

namespace WebJobs.Extensions.Ftp.Trigger;

public abstract class AbstractBaseFtpAttribute : Attribute
{
    public string Connection { get; set; } = null!;

    /// <summary>
    /// Helper method to get connection string from environment variables
    /// </summary>
    internal string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(Connection) ?? throw new ArgumentNullException(nameof(Connection), "Connection is not defined as Environment variable.");
    }
}