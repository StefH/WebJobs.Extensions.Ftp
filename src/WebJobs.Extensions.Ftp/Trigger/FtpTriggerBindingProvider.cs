using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Stef.Validation;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// Binding provider
/// </summary>
internal class FtpTriggerBindingProvider : ITriggerBindingProvider
{
    /// <summary>
    /// FtpExtensionConfigProvider instance variable. Used to create the context.
    /// </summary>
    private readonly FtpExtensionConfigProvider _provider;

    public FtpTriggerBindingProvider(FtpExtensionConfigProvider provider)
    {
        _provider = Guard.NotNull(provider);
    }

    /// <summary>
    /// Create the trigger binding
    /// </summary>
    /// <param name="context"><c>TriggerBindingProviderContext</c> context</param>
    /// <returns>A Task that has the trigger binding</returns>
    public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
    {
        Guard.NotNull(context);

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

        Guard.Condition(attribute, a => a.PollingIntervalInSeconds > 0, nameof(FtpTriggerAttribute.PollingIntervalInSeconds));
        Guard.Condition(attribute, a => a.BatchSize > 0, nameof(FtpTriggerAttribute.BatchSize));

        var triggerBinding = new FtpTriggerBinding(parameter.ParameterType, _provider.CreateContext(attribute));

        return Task.FromResult<ITriggerBinding>(triggerBinding);
    }
}