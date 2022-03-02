using System;
using Stef.Validation;

namespace WebJobs.Extensions.Ftp.Trigger;

public abstract class AbstractBaseFtpAttribute : Attribute
{
    public string Connection { get; set; } = null!;

    protected AbstractBaseFtpAttribute()
    {
    }

    protected AbstractBaseFtpAttribute(string connection)
    {
        Connection = Guard.NotNullOrEmpty(connection);
    }

    /// <summary>
    /// Helper method to get ConnectionString from environment variable.
    /// If that fails, use the ConnectionString as-is.
    /// Else throw.
    /// </summary>
    internal string GetConnectionString()
    {
        return Guard.NotNullOrEmpty(Environment.GetEnvironmentVariable(Connection) ?? Connection);
    }
}