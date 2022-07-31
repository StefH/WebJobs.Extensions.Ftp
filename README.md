# WebJobs.Extensions.Ftp

An extension for WebJobs which uses [FluentFTP](https://github.com/robinrodricks/FluentFTP) to support:
- Ftp Trigger on a new FtpFile, FtpFile[], FtpStream and FtpStream[]. 
- Ftp Bindings on: IAsyncCollector, IFtpClient, FtpFile and FtpStream

## NuGet
[![NuGet Badge](https://buildstats.info/nuget/WebJobs.Extensions.Ftp)](https://www.nuget.org/packages/WebJobs.Extensions.Ftp)

You can install from NuGet using the following command in the package manager window:

`Install-Package WebJobs.Extensions.Ftp`

Or via the Visual Studio NuGet package manager or if you use the `dotnet` command:

`dotnet add package WebJobs.Extensions.Ftp`

## :books: Documentation
- [Custom Azure Function Extension - Part 1 - Triggers](./doc/part1.md)
- [Custom Azure Function Extension - Part 2 - Bindings](./doc/part2.md)


## :wrench: Usage

### Configuration
Add a `FtpConnection` property with the url to the FTP server.

``` json
{
    "Values": {
        "FtpConnection": "ftp://testuser:mypwd@localhost"
    }
}
```

### Ftp Triggers

#### Trigger on FtpItem
``` c#
[FunctionName("FtpTriggerFtpFile")]
public static void RunFtpTriggerFtpFile(
    [FtpTrigger(Connection = "FtpConnection", Folder = "inbox", PollingInterval = "30s")] FtpFile ftpItem,
    ILogger log)
{
    log.LogInformation($"{ftpItem.GetType()} {ftpItem.Name} {ftpItem.FullName} {ftpItem.Size} {ftpItem.Content?.Length}");
}
```

#### Trigger on FtpStream
``` c#
[FunctionName("FtpTriggerFtpStream")]
public static async Task RunFtpTriggerFtpStream(
    [FtpTrigger(Connection = "FtpConnection", Folder = "inbox")] FtpStream ftpStream,
    ILogger log)
{
    log.LogInformation($"{ftpStream.GetType()} {ftpStream.Name} {ftpStream.FullName} {ftpStream.Size} {ftpStream.Stream?.Length}");
}
```


### Ftp Binding

#### Binding on FtpFile
``` c#
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
```

#### Binding on FtpStream
``` c#
[FunctionName("FtpBindingFtpStream")]
public static void RunBindingFtpStream(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    [Ftp(Connection = "FtpConnection", Folder = "inbox")] out FtpStream item,
    ILogger log)
{
    string msg = req.Query["message"];

    log.LogInformation($"Received message {msg}");

    item = new FtpStream
    {
        Name = "stef1.txt",
        Stream = new MemoryStream(Encoding.UTF8.GetBytes(msg))
    };
}
```

#### Binding on IAsyncCollector
``` c#
[FunctionName("BindingIAsyncCollector")]
public static async Task RunBindingIAsyncCollector(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    [Ftp(Connection = "FtpConnection", Folder = "inbox")] IAsyncCollector<FtpFile> collector,
    ILogger log)
{
    string msg = req.Query["message"];

    log.LogInformation($"Received message {msg}");
    
    var item = new FtpFile
    {
        Name = "stef2.txt",
        Content = Encoding.UTF8.GetBytes(msg)
    };

    await collector.AddAsync(item);
}
```

### Combine Ftp Trigger and Ftp Binding
The following example shows how to combine a trigger and a binding to process and delete a file.

The `IFtpClient` is exposed here, which allows full access to the FTP server.
``` c#
[FunctionName("FtpTriggerSampleWithClient")]
public static void RunFtpTriggerSampleWithClient(
    [FtpTrigger(Connection = "FtpConnection", Folder = "inbox", PollingIntervalInSeconds = 30, IncludeContent = false)] FtpFile ftpItem,
    [Ftp(Connection = "FtpConnection", Folder = "inbox")] IFtpClient client,
    ILogger log)
{
    // Do some processing with the FtpFile ...

    client.DeleteFile(ftpItem.FullName);
}
```

---

## References
 - https://github.com/krvarma/azure-functions-nats-extension
