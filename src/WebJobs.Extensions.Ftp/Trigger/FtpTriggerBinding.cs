using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Stef.Validation;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// Ftp Binding class
/// </summary>
internal class FtpTriggerBinding : ITriggerBinding
{
    private readonly FtpTriggerContext _context;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="parameterType"></param>
    /// <param name="context">FtpTriggerContext instance</param>
    public FtpTriggerBinding(Type parameterType, FtpTriggerContext context)
    {
        TriggerValueType = Guard.NotNull(parameterType);
        _context = Guard.NotNull(context);
    }

    /// <summary>
    /// Trigger value type
    /// </summary>
    public Type TriggerValueType { get; }
        
    /// <summary>
    /// BindingDataContract
    /// </summary>
    public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

    /// <summary>
    /// Bind a value using the binding context
    /// </summary>
    /// <param name="value">Value to bind</param>
    /// <param name="context">Binding contract to use</param>
    /// <returns>Returns a Task that contains the <c>TriggerData</c></returns>
    public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
    {
        var valueProvider = new FtpValueBinder(value);
        var bindingData = new Dictionary<string, object>();
        var triggerData = new TriggerData(valueProvider, bindingData);

        return Task.FromResult<ITriggerData>(triggerData);
    }

    /// <summary>
    /// Create listener class
    /// </summary>
    /// <param name="context">Listener factory context</param>
    /// <returns>A Task that contains the listener instance</returns>
    public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
    {
        var executor = context.Executor;
        var listener = new FtpListener(TriggerValueType, executor, _context);

        return Task.FromResult<IListener>(listener);
    }

    /// <summary>
    /// Get binding description
    /// </summary>
    /// <returns>Returns a string that describes the binding</returns>
    public ParameterDescriptor ToParameterDescriptor()
    {
        return new TriggerParameterDescriptor
        {
            Name = "Ftp",
            DisplayHints = new ParameterDisplayHints
            {
                Prompt = "Ftp",
                Description = "Ftp trigger"
            }
        };
    }
}