using FluentFTP;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// Trigger context class
/// </summary>
internal class FtpTriggerContext
{
    /// <summary>
    /// <c>Attribute</c> instance
    /// </summary>
    public FtpTriggerAttribute FtpTriggerAttribute { get; }
        
    /// <summary>
    /// <c>FtpClient</c> instance to connect FTP
    /// </summary>
    public IFtpClient Client { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="attribute">Attribute instance</param>
    /// <param name="client">FtpClient instance</param>
    public FtpTriggerContext(FtpTriggerAttribute attribute, IFtpClient client)
    {
        FtpTriggerAttribute = attribute;
        Client = client;
    }
}