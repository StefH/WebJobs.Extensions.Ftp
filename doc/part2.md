# Custom Azure Function Extension - Part 2 - Bindings

![FTP Extension](ftp-binding.png)

This article is part two of the two-part series. In my previous article, we looked into how to create a custom trigger. In this article, we will look into how to create a custom binding. 

Since this a continuation of my previous article, I highly recommend reading part one before this one so that you get a basic idea of fundamentals of the custom extension and what are going to build.

For the sake of clarity, I will summarize what we are going to explore. In part one, we understand the basics of custom bindings and how to create a custom trigger. We developed a custom FTP Trigger and a sample application to test the trigger in part one.

In this article, we will look into how to create a custom FTP binding.

# Custom binding
To create a custom binding:

1.  Define a class that extends from `Attribute`.

2.  Create a class that extends the `IAsyncCollector` interface. This interface defines methods to `AddAsync`. The system will call the `AddAsync` function to send data to external resources.

3.  Create a class that implements the IConverter interface. This interface has one method:
    - `Convert`: - The system calls this method to create the AsyncCollector class.

4.  Create a class that implements the interface IExtensionConfigProvider. Similar to Triggers, the system will call the Initialize method. In this method, we bind the attribute class using the AddBindingRule method and bind to the Collector using the AddToCollector method.

Similar to Triggers, when the system starts, it searches for a class that implements IWebJobStartup. When it found a class that implements the interface:

1.  The system calls the Configure method passing the IWebJobsBuilder object. We add the extension using the AddExtension method using the class that implements the IExtensionConfigProvider interface.

2.  The system calls the Initialise method of the IExtensionConfigProvider passing ExtensionConfigContext as a parameter. Our implementation of the Initialize method adds the add the binding rule using the AddBindingRule method of the ExtensionConfigContext, which returns a BindingRule object. We call the BindToCollector to add our binding, passing the Converter as a parameter.

3.  The system calls the Convert method of the ICoverter. Our implementation creates an instance of AsyncCollector class and return.

4.  The systems call AddAsync function to send data to external services.

# Creating Custom binding

As stated before, we are creating a FTP extension here. We will use Visual Studio for development. We will also use the [FluentFTP](https://github.com/robinrodricks/FluentFTP) library.

A custom Extension is a .NET Standard 2.1 Library which needs these packages from NuGet:
- Microsoft.Azure.WebJobs.Extensions
- FluentFTP

Creating binding is relatively simple. As mentioned in the previous section,
we need to create attribute class, converter class, and async collector class.

Here is our `FtpAttribute` class (which extends the `AbstractBaseFtpAttribute`):
``` c#
public sealed class FtpAttribute : AbstractBaseFtpAttribute
{
    /// <summary>
    /// The folder to listen for. Is optional.
    /// </summary>
    public string? Folder { get; set; }

    /// <summary>
    /// Call Connect() on the bound IFtpClient
    ///
    /// Default value is <c>false</c>.
    /// </summary>
    public bool AutoConnectFtpClient { get; set; }

    /// <summary>
    /// Cache the FtpClient based on the connection-string.
    ///
    /// In case of <c>false</c>, the caller must the dispose FtpClient manually.
    ///
    /// Default value is <c>true</c>.
    /// </summary>
    public bool CacheFtpClient { get; set; } = true;

    public FtpAttribute()
    {
    }

    public FtpAttribute(string connection) : base(connection)
    {
    }
}
```

Just like in the trigger, we have connection string (from the base-class) and a folder member variables. Next is our converter class. In this class, we create our async collector instance. Here is our async collector class. Just like in the trigger, we will generate context and pass it to the Collector. The 
Collector will use this instance to send a message to the FTP server. Here is our collector class.
``` c#
internal class FtpAsyncCollector<T> : IAsyncCollector<T>
{
    private readonly ILogger _logger;

    private readonly FtpBindingContext _context;

    public FtpAsyncCollector(ILogger logger, FtpBindingContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// UploadAsync to FTP
    /// </summary>
    /// <param name="message">Message to be published</param>
    /// <param name="cancellationToken">A Cancellation Token</param>
    /// <returns>A Task that completes when the message us published</returns>
    public Task AddAsync(T message, CancellationToken cancellationToken = default)
    {
        //
    }

    private async Task AddFtpFileAsync(FtpFile ftpFile, CancellationToken cancellationToken)
    {
        //
    }

    private async Task AddFtpStreamAsync(FtpStream ftpStream, CancellationToken cancellationToken)
    {
        //
    }

    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private async Task CheckIfConnectedAsync(CancellationToken cancellationToken)
    {
        if (!_context.Client.IsConnected)
        {
            await _context.Client.ConnectAsync(cancellationToken);
        }
    }
}
```

## Create a sample to test the FTP Binding

Let's create a sample function to test our binding. Our sample function looks like this:
``` c#
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Bindings;
using WebJobs.Extensions.Ftp.Models;

namespace Bindings.Sample.Ftp;

public static class FtpBindingsSample
{
    [FunctionName("FtpBindingFtpFile")]
    public static void RunBindingFtpFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        [Ftp(Connection = "FtpConnection", Folder = "inbox")] out FtpFile item,
        ILogger log)
    {
        string msg = req.Query["message"];

        log.LogInformation($"Received message {msg}");

        item = new FtpFile
        {
            Name = "stef1.txt",
            Content = Encoding.UTF8.GetBytes(msg)
        };
    }
}
```

I hope you enjoy this article and got a preliminary knowledge of how to create custom extensions for Azure Functions.

---

## References
 - https://github.com/krvarma/azure-functions-Ftp-extension