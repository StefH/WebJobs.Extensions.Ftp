using FluentFTP;

namespace WebJobs.Extensions.Ftp.Bindings;

/// <summary>
/// Ftp Binding context. Contains attribute and FTP client instance
/// </summary>
internal class FtpBindingContext
{
    /// <summary>
    /// Ftp Binding attribute.
    /// </summary>
    public FtpAttribute FtpAttribute { get; }

    /// <summary>
    /// <c>FtpClient</c> instance to connect FTP.
    /// </summary>
    public IFtpClient Client { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ftpAttribute">Ftp Binding Attribute</param>
    /// <param name="client">FtpClient</param>
    public FtpBindingContext(FtpAttribute ftpAttribute, IFtpClient client)
    {
        FtpAttribute = ftpAttribute;
        Client = client;
    }
}