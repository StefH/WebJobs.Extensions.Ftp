using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace WebJobs.Extensions.Ftp.Bindings;

internal class FtpBindingConverterForIAsyncCollector<T> : IConverter<FtpAttribute, IAsyncCollector<T>>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Extension Config Provider
    /// </summary>
    private readonly FtpExtensionConfigProvider _provider;

    public FtpBindingConverterForIAsyncCollector(ILogger logger, FtpExtensionConfigProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    /// <summary>
    /// Convert, create the async collector class
    /// </summary>
    /// <param name="input">Ftp attribute instance</param>
    /// <returns>Returns the async collector instance</returns>
    public IAsyncCollector<T> Convert(FtpAttribute input)
    {
        return new FtpAsyncCollector<T>(_logger, _provider.CreateContext(input));
    }
}