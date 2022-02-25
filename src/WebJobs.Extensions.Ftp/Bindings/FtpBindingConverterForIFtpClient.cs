using FluentFTP;
using Microsoft.Azure.WebJobs;
using Stef.Validation;

namespace WebJobs.Extensions.Ftp.Bindings;

internal class FtpBindingConverterForIFtpClient : IConverter<FtpAttribute, IFtpClient>
{
    private readonly FtpExtensionConfigProvider _provider;

    public FtpBindingConverterForIFtpClient(FtpExtensionConfigProvider provider)
    {
        _provider = Guard.NotNull(provider);
    }

    public IFtpClient Convert(FtpAttribute input)
    {
        return _provider.CreateContext(input).Client;
    }
}