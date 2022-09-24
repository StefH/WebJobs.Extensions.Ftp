using System.Collections.Concurrent;
using FluentFTP;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Bindings;
using WebJobs.Extensions.Ftp.Factories;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Trigger;

namespace WebJobs.Extensions.Ftp;

/// <summary>
/// Extension Config Provider class
/// </summary>
internal class FtpExtensionConfigProvider : IExtensionConfigProvider
{
    private readonly ILogger _logger;

    /// <summary>
    /// A ConcurrentDictionary to cache the clients. The IFtpClients are cached based on the connection string.
    /// </summary>
    private readonly ConcurrentDictionary<string, IFtpClient> _clientCache = new();

    public FtpExtensionConfigProvider(ILogger<FtpExtensionConfigProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initialize the extensions
    /// </summary>
    /// <param name="context">Extension config context</param>
    public void Initialize(ExtensionConfigContext context)
    {
        // 1. Add trigger
        var triggerRule = context.AddBindingRule<FtpTriggerAttribute>();
        triggerRule.BindToTrigger(new FtpTriggerBindingProvider(_logger, this));

        // 2. Add bindings
        var bindingRule = context.AddBindingRule<FtpAttribute>();

        // 2a. Add Input-Binding
        bindingRule.BindToInput<IFtpClient>(typeof(FtpBindingConverterForIFtpClient), this);

        // 2b. Add IAsyncCollector Output-Binding for FtpFile and FtpStream
        var arguments = new object[] { _logger, this };
        bindingRule.BindToCollector<FtpFile>(typeof(FtpBindingConverterForIAsyncCollector<>), arguments);
        bindingRule.BindToCollector<FtpStream>(typeof(FtpBindingConverterForIAsyncCollector<>), arguments);
    }

    /// <summary>
    /// Create Trigger context from a new FtpClient and the attribute supplied. (Note that caching is not possible here.)
    /// </summary>
    /// <param name="attribute">FtpTriggerAttribute instance</param>
    /// <returns>FtpTriggerContext instance</returns>
    public FtpTriggerContext CreateContext(FtpTriggerAttribute attribute)
    {
        var connectionString = attribute.GetConnectionString();

        var ftpClient = FtpClientHelper.CreateFtpClient(connectionString);

        return new FtpTriggerContext(attribute, ftpClient);
    }

    /// <summary>
    /// Create Binding Context from a new or cached FtpClient and attribute supplied.
    /// </summary>
    /// <param name="attribute">Ftp Attribute</param>
    /// <returns>Returns FtpBindingContext instance. The </returns>
    public FtpBindingContext CreateContext(FtpAttribute attribute)
    {
        var connectionString = attribute.GetConnectionString();

        var ftpClient = attribute.CacheFtpClient
            ? _clientCache.GetOrAdd(connectionString, cs => FtpClientHelper.CreateFtpClient(cs, attribute.AutoConnectFtpClient))
            : FtpClientHelper.CreateFtpClient(connectionString, attribute.AutoConnectFtpClient);

        return new FtpBindingContext(attribute, ftpClient);
    }
}