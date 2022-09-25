using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Bindings;
using WebJobs.Extensions.Ftp.Models;

namespace Bindings.Sample.Ftp;

public static class FtpBindingsSample
{
    [FunctionName("FtpBindingFtpFile")]
    public static IActionResult RunBindingFtpFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,


        [Ftp(Connection = "FtpConnection", Folder = "inbox")] out FtpFile? item,

        ILogger log)
    {
        if (!req.Query.TryGetValue("message", out var stringValues))
        {
            item = default;
            return new OkObjectResult("Please provide a query parameter 'message' with a value.");
        }

        log.LogInformation($"Received message {stringValues}");

        item = new FtpFile
        {
            Name = "stef-ftpfile.txt",
            Content = Encoding.UTF8.GetBytes(stringValues)
        };

        return new OkObjectResult("FtpFile added");
    }

    [FunctionName("FtpBindingFtpStream")]
    public static IActionResult RunBindingFtpStream(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        [Ftp(Connection = "FtpConnection", Folder = "inbox")] out FtpStream? item,
        ILogger log)
    {
        if (!req.Query.TryGetValue("message", out var stringValues))
        {
            item = default;
            return new OkObjectResult("Please provide a query parameter 'message' with a value.");
        }

        log.LogInformation($"Received message {stringValues}");

        item = new FtpStream
        {
            Name = "stef-ftpstream.txt",
            Stream = new MemoryStream(Encoding.UTF8.GetBytes(stringValues))
        };

        return new OkObjectResult("FtpStream added");
    }

    [FunctionName("BindingIAsyncCollector")]
    public static async Task<IActionResult> RunBindingIAsyncCollector(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,


        [Ftp("FtpConnection", Folder = "inbox")] IAsyncCollector<FtpFile> collector,

        ILogger log)
    {
        if (!req.Query.TryGetValue("message", out var stringValues))
        {
            return new OkObjectResult("Please provide a query parameter 'message' with a value.");
        }

        log.LogInformation($"Received message {stringValues}");

        var item = new FtpFile
        {
            Name = "stef-asynccollector.txt",
            Content = Encoding.UTF8.GetBytes(stringValues)
        };

        await collector.AddAsync(item);

        return new OkObjectResult("FtpFile added to IAsyncCollector.");
    }
}