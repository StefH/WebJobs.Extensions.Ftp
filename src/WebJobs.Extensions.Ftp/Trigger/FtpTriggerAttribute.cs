using System;
using Microsoft.Azure.WebJobs.Description;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// <c>Attribute</c> class for Trigger
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Binding]
public class FtpTriggerAttribute : AbstractBaseFtpAttribute
{
    public string? Folder { get; set; }

    public int PollingIntervalInSeconds { get; set; } = 60;

    /// <summary>
    /// If true, the FTP Trigger is invoked when the function starts and if a new file is present. 
    /// </summary>
    public bool RunOnStartup { get; set; } = true;

    /// <summary>
    /// Gets files within subdirectories as well. Adds the -r option to the LIST command. Some servers may not support this feature.
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// Include the content from the FtpFile.
    ///
    /// Default value is <c>true</c>.
    /// </summary>
    public bool IncludeContent { get; set; } = true;
}