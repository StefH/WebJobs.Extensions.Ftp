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
    /// The folder to listen on. Is optional.
    /// </summary>
    public string? Folder { get; set; }

    public FtpAttribute()
    {
    }

    public FtpAttribute(string connection) : base(connection)
    {
    }
}