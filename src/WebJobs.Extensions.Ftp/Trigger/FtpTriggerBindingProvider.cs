using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// Binding provider
/// </summary>
internal class FtpTriggerBindingProvider : ITriggerBindingProvider
{
    private readonly ILogger _logger;

    /// <summary>
    /// FtpExtensionConfigProvider instance variable. Used to create the context.
    /// </summary>
    private readonly FtpExtensionConfigProvider _provider;

    public FtpTriggerBindingProvider(ILogger logger, FtpExtensionConfigProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    /// <summary>
    /// Create the trigger binding
    /// </summary>
    /// <param name="context"><c>TriggerBindingProviderContext</c> context</param>
    /// <returns>A Task that has the trigger binding</returns>
    public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
    {
        var parameter = context.Parameter;

        var attribute = parameter.GetCustomAttribute<FtpTriggerAttribute>(false);
        if (attribute == null)
        {
            return Task.FromResult<ITriggerBinding>(null!);
        }

        if (!Constants.SupportedTypes.Contains(parameter.ParameterType))
        {
            throw new InvalidOperationException($"Invalid ParameterType. Only {string.Join(",", Constants.SupportedTypes.Select(t => t.Name))} are supported.");
        }

        if (string.IsNullOrEmpty(attribute.PollingInterval))
        {
            throw new ArgumentException("Argument is null or empty", nameof(FtpTriggerAttribute.PollingInterval));
        }

        if (attribute.BatchSize <= 0)
        {
            throw new ArgumentException("Argument is must be > 0", nameof(FtpTriggerAttribute.BatchSize));
        }

        var triggerBinding = new FtpTriggerBinding(_logger, parameter.ParameterType, _provider.CreateContext(attribute));

        return Task.FromResult<ITriggerBinding>(triggerBinding);
    }
}