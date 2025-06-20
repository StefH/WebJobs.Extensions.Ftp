## WebJobs.Extensions.Ftp

An extension for WebJobs which uses [FluentFTP](https://github.com/robinrodricks/FluentFTP) to support:
- *Ftp Trigger* on a new `FtpFile`, `FtpFile[]`, `FtpStream` and `FtpStream[]`. 
- *Ftp Bindings* on: `IAsyncCollector`, `IFtpClient`, `FtpFile` and `FtpStream`.

### :wrench: Usage

#### Configuration
Add a `FtpConnection` property with the url to the FTP server.

``` json
{
    "Values": {
        "FtpConnection": "ftp://testuser:mypwd@localhost"
    }
}
```

#### Ftp Triggers

##### Trigger on FtpItem
``` c#
[FunctionName("FtpTriggerFtpFile")]
public static void RunFtpTriggerFtpFile(
    [FtpTrigger(Connection = "FtpConnection", Folder = "inbox", PollingInterval = "30s")] FtpFile ftpItem,
    ILogger log)
{
    log.LogInformation($"{ftpItem.GetType()} {ftpItem.Name} {ftpItem.FullName} {ftpItem.Size} {ftpItem.Content?.Length}");
}
```

##### Trigger on FtpStream
``` c#
[FunctionName("FtpTriggerFtpStream")]
public static async Task RunFtpTriggerFtpStream(
    [FtpTrigger(Connection = "FtpConnection", Folder = "inbox")] FtpStream ftpStream,
    ILogger log)
{
    log.LogInformation($"{ftpStream.GetType()} {ftpStream.Name} {ftpStream.FullName} {ftpStream.Size} {ftpStream.Stream?.Length}");
}
```


#### Ftp Binding

##### Binding on FtpFile
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

##### Binding on FtpStream
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

##### Binding on IAsyncCollector
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

#### Combine Ftp Trigger and Ftp Binding
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

### Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **WebJobs.Extensions.Ftp**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)