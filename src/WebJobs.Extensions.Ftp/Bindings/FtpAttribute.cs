using System;
using Microsoft.Azure.WebJobs.Description;
using WebJobs.Extensions.Ftp.Trigger;

namespace WebJobs.Extensions.Ftp.Bindings;

/// <summary>
/// <c>Attribute</c> class for Binding
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Binding]
public sealed class FtpAttribute : AbstractBaseFtpAttribute
{
    /// <summary>
    /// The folder to listen for. Is optional.
    /// </summary>
    public string? Folder { get; set; }

    /// <summary>
    /// Call Connect() on the bound IFtpClient
    ///
    /// Default value is <c>false</c>.
    /// </summary>
    public bool AutoConnectFtpClient { get; set; }

    /// <summary>
    /// Cache the FtpClient based on the connection-string.
    ///
    /// In case of <c>false</c>, the caller must the dispose FtpClient manually.
    ///
    /// Default value is <c>true</c>.
    /// </summary>
    public bool CacheFtpClient { get; set; } = true;

    public FtpAttribute()
    {
    }

    public FtpAttribute(string connection) : base(connection)
    {
    }
}