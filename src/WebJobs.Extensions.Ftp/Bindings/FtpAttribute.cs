using System;
using Microsoft.Azure.WebJobs.Description;
using WebJobs.Extensions.Ftp.Trigger;

namespace WebJobs.Extensions.Ftp.Bindings;

/// <summary>
/// <c>Attribute</c> class for Trigger
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Binding]
public class FtpAttribute : AbstractBaseFtpAttribute
{
    public string? Folder { get; set; }
}