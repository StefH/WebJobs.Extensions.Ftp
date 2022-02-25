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
}