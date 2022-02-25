using FluentFTP;
using Stef.Validation;

namespace WebJobs.Extensions.Ftp.Bindings;

/// <summary>
/// Ftp Binding context. Contains attribute and FTP client instance
/// </summary>
internal class FtpBindingContext
{
    /// <summary>
    /// Ftp Binding attribute
    /// </summary>
    public FtpAttribute FtpAttribute { get; }

    /// <summary>
    /// <c>FtpClient</c> instance to connect FTP
    /// </summary>
    public IFtpClient Client { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ftpAttribute">Nats Binding Attribute</param>
    /// <param name="client">Nats Client</param>
    public FtpBindingContext(FtpAttribute ftpAttribute, IFtpClient client)
    {
        FtpAttribute = Guard.NotNull(ftpAttribute);
        Client = Guard.NotNull(client);
    }
}