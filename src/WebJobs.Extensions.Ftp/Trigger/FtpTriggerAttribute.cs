using System;
using Microsoft.Azure.WebJobs.Description;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// <c>Attribute</c> class for Trigger
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Binding]
public sealed class FtpTriggerAttribute : AbstractBaseFtpAttribute
{
    /// <summary>
    /// The folder to listen on. Is optional.
    /// </summary>
    public string? Folder { get; set; }

    /// <summary>
    /// The maximum number of items returned in case an array is required.
    ///
    /// Default value is <c>32</c>.
    /// </summary>
    public int BatchSize { get; set; } = 32;

    /// <summary>
    /// The polling interval.
    ///
    /// Defined as {number}{s|m|h|d}.
    /// Examples:
    /// - 10s poll every 10 seconds
    /// - 2m  poll every 2 minutes
    ///
    /// Default value is <c>1m</c>. 
    /// </summary>
    public string? PollingInterval { get; set; }

    /// <summary>
    /// Gets files within subdirectories as well. Adds the -r option to the LIST command. Some servers may not support this feature.
    ///
    /// Default value is <c>false</c>.
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// Include the content from the FtpFile.
    ///
    /// Default value is <c>true</c>.
    /// </summary>
    public bool IncludeContent { get; set; } = true;

    /// <summary>
    /// Load the modify date using MDTM when it could not be parsed from the server listing.
    /// This only pertains to servers that do not implement the MLSD command.
    ///
    /// Default value is <c>true</c>.
    /// </summary>
    public bool LoadModifyDateUsingMDTM { get; set; } = true;

    /// <summary>
    /// Force a trigger when it runs for the first time, ignoring the modify date.
    ///
    /// Default value is <c>false</c>.
    /// </summary>
    public bool ForceTriggerOnFirstRun { get; set; }

    public FtpTriggerAttribute()
    {
    }

    public FtpTriggerAttribute(string connection) : base(connection)
    {
    }
}