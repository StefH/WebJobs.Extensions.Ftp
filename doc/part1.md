# Custom Azure Function Extension - Part 1 - Triggers

![FTP Extension](ftp-trigger.png)

## Azure Function Custom Extension
We can use [Azure WebJob SDK](https://github.com/Azure/azure-webjobs-sdk) to develop Azure Functions custom extensions.

There are two types of extensions:
1. Trigger
2. Binding(s)

### Trigger
A trigger causes a function to run. In most of the cases, a trigger will have data associated with it.
The Azure Function receives this trigger data as a `parameter`.

### Binding
A binding is a method to connect other resources to the function.
- Input binding receives the data from the external source
- Output binding sends the data to the external source.

### Attribute class
An Attribute class defines every Trigger and Binding.
An Attribute class defines all the parameters and configuration values for the trigger or extension.
Attribute class is a crucial component in custom extension.
When a consumer defines a trigger or binding, the system looks for a corresponding attribute class and initializes it.

---

## Custom Trigger
To create a custom Trigger, we need to:

 -  Define a class that extends from `Attribute`. This class represents our attribute class. We define all the parameters and configuration values for our trigger. In our case, we define connection string and Ftp channels.

 -  Define a class that implements the interface `IListener`. This class contains the logic to connect to the Ftp server and wait for new Ftp Files.

    The `IListener` interface has the following functions:
     - *StartAsync*:- The system calls this function to start our listener. This function returns one Task object that completes when our listener successfully started.
     - *StopAsync*:- The system calls this function to stop our listener. This function returns one Task object that completes when the listener completely stopped.
     - *Cancel*:- The system calls this function to cancel any ongoing listen operation.
     - *Dispose*:- IDisposable's dispose function.

 -  Define a class that implements the interface `ITriggerBinding`. In this class, we create our listener and bind our trigger data. The ITriggerBinding interface has the following functions:
    - *CreateListenerAsync*:- The system calls this function to create a listener. This function returns a Task object that has our listener.

    - *BindAsync*:- This function is called to bind a specified value using a binding context. When our listener receives an event, we try to execute the function, passing the event data.

      This event data is encapsulated in a `TriggeredFunctionData` class and send to the Azure Function.
      In the `BindAsync`, we will bind this value to our corresponding data. This function returns a `TriggerData` class.
      The `TriggerData` class accepts a class that implements an `IValueBinder` interface and a read-only dictionary, this will revisited later in this article.

    - *ToParameterDescriptor*:- The system calls this function to get a description of the binding.

 -  Define a class that implements the interface `ITriggerBindingProvider`. This class is a provider class that returns a class that implements the `ITriggerBinding` interface. This class has the following methods:
     -  *TryCreateAsync*:- When functions are being discovered this function wil be called to get a class that implements the `ITriggerBinding` interface. The system will pass a `TriggerBindingProviderContext` class as a parameter. In this function, we check whether the `TriggerBindingProviderContext` object contains our custom attribute. If the `Attribute` is present, we will create TriggerBinding class and return a Task.

 -  Define a class that implements the interface `IValueBinder`. As explained in the `BindAsync` section, we are binding the trigger data to our data class using this class. The `IValueBinder` has three methods:
     -  *GetValueAsync*:- Returns a task that has the value object.
     -  *SetValueAsync*: - Returns a task that completes when the object to our data class completes.
     -  *ToInvokeString*:- Returns object string.

 -  Create a class that implements the interface `IExtensionConfigProvider`. The `IExtensionConfigProvider` defines an interface enabling third party extension to register. The interface has the following function:
     -  *Initialize*:- In this function, we will register all our triggers and bindings.

 -  And finally, we create a class that implements the interface `IWebJobStartup`. This interface defines the configuration actions to perform when the Function Host starts up. This interface has the following function:
     -  *Configure*:- The system call this function when the function host initializes. In this function, we will add our custom extension.

So basically what happens is when the system starts, it searches for a class that implements `IWebJobStartup`. When it found a class that implements the interface: 

- The system calls the Configure method passing the `IWebJobsBuilder` object. We add the extension using the `AddExtension` method using the class that implements the `IExtensionConfigProvider` interface.

- The system calls the Initialise method of the `IExtensionConfigProvider` passing `ExtensionConfigContext` as a parameter. Our implementation of the Initialize method adds the add the binding rule using the `AddBindingRule` method of the `ExtensionConfigContext`, which returns a `BindingRule` object. We call the `BindToTrigger` method to add our trigger passing `TriggerBindingProvider` as a parameter.

-  After that system calls the `TryCreateAsync` function of the `TriggerBindingProvider` passing the `TriggerBindingProviderContext` as a parameter, in this `TryCreateAsync` method, we check whether our `Attribute` class present or not. We create our class that implements the `ITriggerBinding` interface and return a Task that contains the object.

- The system then calls the `CreateListenerAsync` method of our class that implements the `ITriggerBinding` interface passing `ListenerFactoryContext` object. In our `CreateListenerAsync`, we return a class that implements the `IListener` interface. The `ListenerFactoryContext` object contains a class that implements the `ITriggeredFunctionExecutor` interface. The `ITriggeredFunctionExecutor` interface has a method called `TryExecuteAsync`. Using this method, we can execute the triggered function, passing the event data and `CancellationToken`.

## Creating FTP Custom Extension
As stated before, we are creating a FTP extension here.
We will use Visual Studio for development.
We will also use the [FluentFTP](https://github.com/robinrodricks/FluentFTP).

A custom Extension is a .NET Standard 2.1 Library which needs these packages from NuGet:
- Microsoft.Azure.WebJobs.Extensions
- FluentFTP

## Create Trigger
First create an `AbstractBaseFtpAttribute`. This is an abstract base class because it's use for trigger and binding attributes.

This class AbstractBaseFtpAttribute looks as follows:
``` c#
public abstract class AbstractBaseFtpAttribute : Attribute
{
    /// <summary>
    /// The Connection represents the FTP connection string.
    /// </summary>
    public string Connection { get; set; } = null!;

    protected AbstractBaseFtpAttribute()
    {
    }

    protected AbstractBaseFtpAttribute(string connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Helper method to get ConnectionString from environment variable.
    /// If that fails, use the ConnectionString as-is.
    /// </summary>
    internal string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(Connection) ?? Connection;
    }
}
```

The `FtpTriggerAttribute` which extends the `AbstractBaseFtpAttribute` and looks as follows:
``` c#
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
    public bool TriggerOnStartup { get; set; }

    /// <summary>
    /// The trigger mode to use.
    ///
    /// Default value is <c>ModifyDate</c>.
    /// </summary>
    public TriggerMode TriggerMode { get; set; } = TriggerMode.ModifyDate;

    public FtpTriggerAttribute()
    {
    }

    public FtpTriggerAttribute(string connection) : base(connection)
    {
    }
}
```

Next, we need to create a `FtpListener` class. See this [link](https://github.com/StefH/WebJobs.Extensions.Ftp/blob/main/src/WebJobs.Extensions.Ftp/Trigger/FtpListener.cs) for the complete code.

The Listener class receives four parameters:
- `ILogger`; The logger
- `Type`; The TriggerValue Type
- `ITriggeredFunctionExecutor`; executor
- `FtpTriggerContext`; context

We use the `ITriggeredFunctionExecutor` instance to execute the triggered function when we receive a message.

The `FtpTriggerContext` object has two member variables:
- The `TriggerAttribute` variable is an object of our `Attribute` class.
- The `Client` variable is an object of the `IFtpClient` interface which represents the FtpClient instance to connect FTP.

Here is context class:
``` c#
internal class FtpTriggerContext
{
    public FtpTriggerAttribute FtpTriggerAttribute { get; }
        
    public IFtpClient Client { get; }

    public FtpTriggerContext(FtpTriggerAttribute attribute, IFtpClient client)
    {
        FtpTriggerAttribute = attribute;
        Client = client;
    }
}
```

The `FtpListener` class uses the `FtpClient` object to subscribe to a Ftp channel. When we receive a message, we invoke the function using the `ITriggeredFunctionExecutor` instance.

Next, we need to create `TriggerBinding` and `TriggerBindingProvider` class. There are relatively simple classes. The `TryCreateAsync` in the `FtpTriggerBindingProvider` class create `FtpTriggerBinding` instance and return. One thing to note here is, creating the context class. We create the `FtpTriggerContext` class instance by calling the `CreateContext` method of the `FtpExtensionConfigProvider` class. We will create the context class and pass it to the `FtpTriggerBinding` object.

Next, we create the `FtpExtensionConfigProvider` class. Inside the Initialize method, we create a rule using the `AddBindingRule` method and bind the binding provider to it. 

Another major thing to note here is that the `IFtpServiceFactory` instance we are receiving in the constructor. We will revisit this in the next section. Just remember that the system will pass this as a parameter.

Next and finally, we need to create a startup class. This startup class looks like this:
``` c#
public class FtpBinding
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddFtp();
        }
    }
}
```

As I stated in the previous section, the `IWebJobsStartup` interface has only one method, `Configure`.
The Configure method takes one parameter, an object of `IWebJobsBuilder` implementation.
The system will pass this parameter to our `Configure` method.
 
You should have noticed the `AddFtp` method.
This method is an extension function of `IWebJobsBuilder` and is in the `FtpWebJobsBuilderExtensions` class.
The `AddFtpExtension` is just a helper method. The `FtpWebJobsBuilderExtensions` class looks like this.
``` c#
public static class FtpWebJobsBuilderExtensions
{
    public static IWebJobsBuilder AddFtp(this IWebJobsBuilder builder)
    {
        builder.AddExtension<FtpExtensionConfigProvider>();

        builder.Services.AddSingleton<IFtpClientFactory, FtpClientFactory>();
        builder.Services.AddLogging();

        return builder;
    }
}
```

As you can see in this extension method, we are adding the extension using the `AddExtension` method of the `IWebJobsBuilder`.
The `AddExtension` method takes one parameter, our `FtpExtensionConfigProvider` instance.
We are also adding a Singleton Service to the builder.
The constructor of the `FtpExtensionConfigProvider` instance will receive this server as a parameter.

---

## Create a sample to test the Ftp Trigger
Now we need to create a sample function to test our trigger.
Let' create a test Azure Function that uses our trigger. Our sample function looks like this:

``` c#
public static class FtpTriggerSample
{
    [FunctionName("RunFtpTriggerFtpFileAlways")]
    public static void RunFtpTriggerFtpFileAlways(
        [FtpTrigger("FtpConnectionAnonymous", Folder = "inbox", PollingInterval = "30s", TriggerMode = TriggerMode.Always)] FtpFile ftpFile,
        ILogger log)
    {
        log.LogInformation($"RunFtpTriggerFtpFileAlways >> {ftpFile.GetType()} {ftpFile.Name} {ftpFile.FullName} {ftpFile.Size} {ftpFile.Content?.Length}");
    }
```

This function is straightforward, just log some informational message.
The `Connection` string is from the `local.settings.json` file, and the `Channel` is hard-coded.

Before running our function, we need to run a Ftp server for testing (e.g. [FileZilla(https://filezilla-project.org/download.php)]).

For more examples, see [Trigger.Sample.Ftp](https://github.com/StefH/WebJobs.Extensions.Ftp/tree/main/examples/Trigger.Sample.Ftp).

---

In the next part, we will look into how to create **Ftp Bindings**, till then Happy Coding!.

---

## References
 - https://github.com/krvarma/azure-functions-nats-extension
 - https://www.tpeczek.com/2018/11/azure-functions-20-extensibility_20.html
 - https://geradegeldenhuys.net/read/custom-azure-function-trigger/