# Custom Azure Function Extension - Part 1 - Triggers

_plaatje..._

## Azure Function Custom Extension
We can use [Azure WebJob SDK](https://github.com/Azure/azure-webjobs-sdk) to develop Azure Functions custom extensions.

There are two types of extensions:
1. Trigger
2. Binding(s)

The **trigger** causes a function to run. In most of the cases, a trigger will have data associated with it. The Azure Function receives this trigger data as a parameter.

A **binding** is a method to connect other resources to the function.
Input binding receives the data from the external source, and an output binding sends the data to the external source.

An Attribute class defines every Trigger and Binding.
An Attribute class defines all the parameters and configuration values for the trigger or extension.
Attribute class is a crucial component in custom extension.
When a consumer defines a trigger or binding, the system looks for a corresponding attribute class and initialize it.

## Custom Trigger
To create a custom Trigger, we need to:

 -  Define a class that extends from Attribute. This class represents our attribute class. We define all the parameters and configuration values for our trigger. In our case, we define connection string and Ftp channels.
 -  Define a class that implements the interface `IListener`. This class contains the logic to connect to our external event source and wait for events. In our case, we will connect to the Ftp server and look for incoming messages. The IListener interface has the following functions:
	 - *StartAsync*:- The system calls this function to start our listener. This function returns one Task object that completes when our listener successfully started.
	 - *StopAsync*:- The system calls this function to stop our listener. This function returns one Task object that completes when the listener completely stopped.
	 - *Cancel*:- The system calls this function to cancel any ongoing listen operation.
	 - *Dispose*:- IDisposable's dispose function.

 -  Define a class that implements the interface `ITriggerBinding`. In this class, we create our listener and bind our trigger data. The ITriggerBinding interface has the following functions:
	-  *CreateListenerAsync*:- The system calls this function to create a listener. This function returns a Task object that has our listener.
	-  *BindAsync*:- This function is called to bind a specified value using a binding context. When our listener receives an event, we try to execute the function, passing the event data. This event data is encapsulated in a `TriggeredFunctionData` class and send to the Azure Function. In the `BindAsync`, we will bind this value to our corresponding data. This function returns a `TriggerData` class. `TriggerData` class accepts a class that implements an `IValueBinder` interface and a read-only dictionary. We will revisit this later in this article.
	-  *ToParameterDescriptor*:- The system calls this function to get a description of the binding.

 -  Define a class that implements the interface `IValueBinder`. As I explained in the `BindAsync` section, we are binding the trigger data to our data class using this class. The `IValueBinder` has three methods:

	 -  *GetValueAsync*:- Returns a task that has the value object.
	 -  *SetValueAsync*: - Returns a task that completes when the object to our data class completes.
	 -  *ToInvokeString*:- Returns object string.

 -  Define a class that implements the interface `ITriggerBindingProvider`. This class is a provider class that returns a class that implements the `ITriggerBinding` interface. This class has the following function:

	 -  *TryCreateAsync*:- The system call this function to get a class that implements the `ITriggerBinding` interface. The system will pass a `TriggerBindingProviderContext` class as a parameter. In this function, we check whether the `TriggerBindingProviderContext` object contains our custom attribute. If the `Attribute` is present, we will create TriggerBinding class and return a Task.

 -  Create a class that implements the interface `IExtensionConfigProvider`. The `IExtensionConfigProvider` defines an interface enabling third party extension to register. The interface has the following function:

	 -  *Initialize*:- In this function, we will register all our triggers and bindings.

 -  And finally, we create a class that implements the interface `IWebJobStartup`. This interface defines the configuration actions to perform when the Function Host starts up. This interface has the following function:

	 -  *Configure*:- The system call this function when the function host initializes. In this function, we will add our custom extension.

So basically what happens is when the system starts, it searches for a class that implements  `IWebJobStartup`. When it found a class that implements the interface: 

 - The system calls the Configure method passing the `IWebJobsBuilder` object. We add the extension using the `AddExtension` method using the class that implements the `IExtensionConfigProvider` interface.
- The system calls the Initialise method of the `IExtensionConfigProvider` passing `ExtensionConfigContext` as a parameter. Our implementation of the Initialize method adds the add the binding rule using the `AddBindingRule` method of the `ExtensionConfigContext`, which returns a `BindingRule` object. We call the `BindToTrigger` method to add our trigger passing `TriggerBindingProvider` as a parameter.
-  After that system calls the `TryCreateAsync` function of the `TriggerBindingProvider` passing the `TriggerBindingProviderContext` as a parameter, in this `TryCreateAsync` method, we check whether our `Attribute` class present or not. We create our class that implements the `ITriggerBinding` interface and return a Task that contains the object.
- The system then calls the `CreateListenerAsync` method of our class that implements the `ITriggerBinding` interface passing `ListenerFactoryContext` object. In our `CreateListenerAsync`, we return a class that implements the `IListener` interface. The `ListenerFactoryContext` object contains a class that implements the `ITriggeredFunctionExecutor` interface. The `ITriggeredFunctionExecutor` interface has a method called `TryExecuteAsync`. Using this method, we can execute the triggered function, passing the event data and `CancellationToken`.

## Creating FTP Custom Extension
As stated before, we are creating a FTP extension here.
We will use Visual Studio for development.
We will also use the [FluentFTP](https://github.com/robinrodricks/FluentFTP).

Custom Extension is a .NET Standard 2.1 Library. You need to add the following packages using NuGet.
Open the NuGet manager and search for:
```
    Microsoft.Azure.WebJobs.Extensions
    FluentFTP
```

## Create Trigger
First create an `Attribute`. This attribute class FtpTriggerAttribute is as follows:
``` c#
    using System;
    using Microsoft.Azure.WebJobs.Description;
    
    namespace WebJobs.Extensions.Ftp
    {
	    /// <summary>
	    /// <c>Attribute</c> class for Trigger
	    /// </summary>
        [AttributeUsage(AttributeTargets.Parameter)]
        [Binding]
        public class FtpTriggerAttribute: Attribute
        {
            // <summary>
            // Connection string in the form of Ftp://<username>:<password>@localhost
            // </summary>
            public string Connection { get; set; }
            // Channel string
            public string Channel { get; set; }
    
            // <siummary>
            // Helper method to get connection string from environment variables
            // </summary>
            internal string GetConnectionString()
            {
                return Environment.GetEnvironmentVariable(Connection);
            }
        }
    }
```

The class has several members, e.g. the required `Connection` and optional `Folder`.
The Connection represents the FTP connection string, and the Folder represents the FTP folder to listen.
There is a helper method, `GetEnvironmentVariable`, also defined in the class, which will retrieve the connection string from the environment variable and return.

Next, we need to create a `FtpListener` class. Here is our listener class:

    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Host.Executors;
    using Microsoft.Azure.WebJobs.Host.Listeners;
    using MyFtpClient.Rx;
    
    namespace WebJobs.Extensions.Ftp
    {
        /*
            The FtpListner class
            Implements the <c>IListener</c> interface. Contains the code to connect
            to a Ftp server and subscribe a Channel.
         */
        public class FtpListener: IListener
        {
            private readonly ITriggeredFunctionExecutor _executor;
            private readonly FtpTriggerContext _context;
    
            /// <summary>
            /// FtpListener constructor
            /// </summary>
            ///
            /// <param name="executor"><c>ITriggeredFunctionExecutor</c> instance</param>
            /// <param name="context"><c>FtpTriggerContext</c> instance</param>
            ///
            public FtpListener(ITriggeredFunctionExecutor executor, FtpTriggerContext context)
            {
                _executor = executor;
                _context = context;
            }
    
            /// <summary>
            /// Cancel any pending operation
            /// </summary>
            public void Cancel()
            {
                if (_context == null || _context.Client == null) return;
    
                _context.Client.Disconnect();
            }
    
            /// <summary>
            ///  Dispose method
            /// </summary>
            public void Dispose()
            {
                _context.Client.Dispose();
            }
    
            /// <summary>
            /// Start the listener asynchronously. Subscribe to Ftp channel and
            /// wait for message. When a message is received, execute the function
            /// </summary>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>A Task returned from Subscribe method</returns>
            public Task StartAsync(CancellationToken cancellationToken)
            {
                return _context.Client.Subscribe(_context.TriggerAttribute.Channel, stream => stream.Subscribe(msg => {
                    var triggerData = new TriggeredFunctionData
                    {
                        TriggerValue = msg.GetPayloadAsString()
                    };
    
                    var task = _executor.TryExecuteAsync(triggerData, CancellationToken.None);
                    task.Wait();
                }));
            }
    
            /// <summary>
            /// Stop current listening operation
            /// </summary>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns></returns>
            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.Run(() =>{
                    _context.Client.Disconnect();
                });
            }
        }
    }

As you can see, the Listener class receives two parameters,  `ITriggeredFunctionExecutor` and a `FtpTriggerContext` instance. 

We use the `ITriggeredFunctionExecutor` instance to execute the triggered function when we receive a message.

The  `FtpTriggerContext` object has two member variables. The `TriggerAttribute` variable is an object of our `Attribute` class. The `Client` variable is an object of the `FtpClient` class, which is a wrapper class around the `MyFtpClient` library. Here is context class:

    namespace WebJobs.Extensions.Ftp
    {
        /// <summary>
        /// Trigger context class
        /// </summary>
        public class FtpTriggerContext
        {
            /// <summary>
            /// <c>Attribute</c> instance
            /// </summary>
            public FtpTriggerAttribute TriggerAttribute;
            /// <summary>
            /// <c>FtpClient</c> instance to connect and subscribe to Ftp
            /// </summary>
            public FtpClient Client;
    
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="attribute">Attribute instnace</param>
            /// <param name="client">FtpClient instance</param>
            public FtpTriggerContext(FtpTriggerAttribute attribute, FtpClient client)
            {
                this.TriggerAttribute = attribute;
                this.Client = client;
            }
        }
    }

The `FtpListener` class uses the `FtpClient` object to subscribe to a Ftp channel. When we receive a message, we invoke the function using the `ITriggeredFunctionExecutor` instance.

Next, we need to create `TriggerBinding` and `TriggerBindingProvider` class. There are relatively simple classes. The `TryCreateAsync` in the `FtpTriggerBindingProvider` class create `FtpTriggerBinding` instance and return. One thing to note here is, creating the context class. We create the `FtpTriggerContext` class instance by calling the `CreateContext` method of the `FtpExtensionConfigProvider` class. We will create the context class and pass it to the `FtpTriggerBinding` object.

Next, we create the `FtpExtensionConfigProvider` class. Inside the Initialize method, we create a rule using the `AddBindingRule` method and bind the binding provider to it. 

Another major thing to note here is that the `IFtpServiceFactory` instance we are receiving in the constructor. We will revisit this in the next section. Just remember that the system will pass this as a parameter.

Next and finally, we need to create a startup class. Our startup class looks like this:

    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;
    using WebJobs.Extensions.Ftp;
    
    [assembly: WebJobsStartup(typeof(FtpBinding.Startup))]
    namespace WebJobs.Extensions.Ftp
    {
        /// <summary>
        /// Starup object
        /// </summary>
        public class FtpBinding
        {
            /// <summary>
            /// IWebJobsStartup startup class
            /// </summary>
            public class Startup : IWebJobsStartup
            {
                public void Configure(IWebJobsBuilder builder)
                {
                    // Add Ftp extensions
                    builder.AddFtpExtension();
                }
            }
        }
    }

As I stated in the previous section, the `IWebJobsStartup` interface has only one method, `Configure`. The Configure method takes one parameter, an object of `IWebJobsBuilder` implementation. The system will pass this parameter to our `Configure` method.
 
You should have noticed the `AddFtpExtension` function. This function is an extension function of `IWebJobsBuilder` and is in the `FtpWebJobsBuilderExtensions` class. The `AddFtpExtension` is just a helper method. The `FtpWebJobsBuilderExtensions` class looks like this.

    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.DependencyInjection;
    
    namespace WebJobs.Extensions.Ftp
    {
        /// <summary>
        /// WebJobBuilder extension to add Ftp extensions
        /// </summary>
        public static class FtpWebJobsBuilderExtensions
        {
            /// <summary>
            /// Extension method to add our custom extensions
            /// </summary>
            /// <param name="builder"><c>IWebJobsBuilder</c> instance</param>
            /// <returns><c>IWebJobsBuilder</c> instance</returns>
            /// <exception>Throws ArgumentNullException if builder is null</exception>
            public static IWebJobsBuilder AddFtpExtension(this IWebJobsBuilder builder)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }
    
    
                builder.AddExtension<FtpExtensionConfigProvider>();
    
                builder.Services.AddSingleton<IFtpServiceFactory, FtpServiceFactory>();
    
                return builder;
            }
        }
    }

As you can see in this extension method, we are adding the extension using the `AddExtension` method of the `IWebJobsBuilder`. The `AddExtesion` method takes one parameter, our `IExtensionConfigProvier` instance. We are also adding a Singleton Service to the builder. The constructor of the `IExtensionConfigProvider` instance will receive this server as a parameter.

We can now build the library. If everything goes well, you can see the DLL file in the BIN folder.

## Create a sample to test the Ftp Trigger
Now we need to create a sample function to test our trigger. Let' create a test Azure Function that uses our trigger. Our sample function looks like this:

    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using WebJobs.Extensions.Ftp;
    
    namespace FtpTrigger.Sample
    {
        public static class FtpTriggerSample
        {
            [FunctionName("FtpTriggerSample")]
            public static void Run(
                [FtpTrigger(Connection = "FtpConnection", Channel = "SampleChannel")] string message,
                ILogger log)
            {
                log.LogInformation($"Message Received From SampleChannel {message}");
            }
        }
    }

This function is straightforward, just log the message we are getting. The `Connection` string is from the `local.settings.json` file, and the `Channel` is hard-coded.

Before running our function, we need to run the Ftp server. Run the server with the following command

    docker run -d --name Ftp-main -p 4222:4222 -p 6222:6222 -p 8222:8222 Ftp

Once it is running, let's start our function. I am using the following `node.js` application to send a message to a Ftp channel. 

    #!/usr/bin/env node
    
    /* jslint node: true */
    'use strict';
    
    var Ftp = require('Ftp').connect("Ftp://<username>:<password>@localhost:4222");
    var args = process.argv.slice(2)
    
    Ftp.on('error', function(e) {
        console.log('Error [' + Ftp.options.url + ']: ' + e);
        process.exit();
    });
    
    var subject = args[0];
    var msg = args[1];
    
    if (!subject) {
        console.log('Usage: node-pub <subject> [msg]');
        process.exit();
    }
    
    Ftp.publish(subject, msg, function() {
        console.log('Published [' + subject + '] : "' + msg + '"');
        process.exit();
    });

Publish a message to the Channel using the following command:

    node.js publish.js SampleChannel "Aure Function and Ftp are awesome."

If everything goes well, you can see the debug log from the function.

![enter image description here](https://raw.githubusercontent.com/krvarma/azure-functions-Ftp-extension/master/images/Ftptrigger.gif)

In the next part, we will look into how to create Ftp bindings, till then Happy Coding!.
