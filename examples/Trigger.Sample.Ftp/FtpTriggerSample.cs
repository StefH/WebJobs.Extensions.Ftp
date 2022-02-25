using System.IO;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Bindings;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Trigger;

namespace Trigger.Sample.Ftp;

public static class FtpTriggerSample
{
    [FunctionName("FtpTriggerFtpFile")]
    public static void RunFtpTriggerFtpFile(
        [FtpTrigger(Connection = "FtpConnection", Folder = "inbox", PollingIntervalInSeconds = 10)] FtpFile ftpItem,
        ILogger log)
    {
        log.LogInformation($"RunFtpTriggerFtpFile >> {ftpItem.GetType()} {ftpItem.Name} {ftpItem.FullName} {ftpItem.Size} {ftpItem.Content?.Length}");
    }

    [FunctionName("FtpTriggerFtpFiles")]
    public static void RunFtpTriggerFtpFiles(
        [FtpTrigger(Connection = "FtpConnection", Folder = "inbox", PollingIntervalInSeconds = 10)] FtpFile[] ftpItem,
        ILogger log)
    {
        log.LogWarning($"RunFtpTriggerFtpFiles >> {ftpItem.GetType()} {ftpItem.Length}");
    }

    [FunctionName("FtpTriggerFtpStream")]
    public static async Task RunFtpTriggerFtpStream(
        [FtpTrigger(Connection = "FtpConnection", Folder = "inbox", PollingIntervalInSeconds = 10)] FtpStream ftpStream,
        ILogger log)
    {
        log.LogInformation($"FtpTriggerFtpStream >> {ftpStream.GetType()} {ftpStream.Name} {ftpStream.FullName} {ftpStream.Size} {ftpStream.Stream?.Length}");

        if (ftpStream.Stream != null)
        {
            log.LogInformation($"FtpTriggerFtpStream >> CanRead={ftpStream.Stream.CanRead}");

            await using var mem = new MemoryStream();
            await ftpStream.Stream.CopyToAsync(mem);

            var bytes = mem.ToArray();
            log.LogInformation($"FtpTriggerFtpStream >> bytes={bytes.Length}");
        }
    }

    [FunctionName("FtpTriggerSampleWithClient")]
    public static void RunFtpTriggerSampleWithClient(
        [FtpTrigger(Connection = "FtpConnection", Folder = "inbox", PollingIntervalInSeconds = 30, IncludeContent = false)] FtpFile ftpItem,
        [Ftp(Connection = "FtpConnection", Folder = "inbox")] IFtpClient client,
        ILogger log)
    {
        // Do some processing with the FtpFile ...

        client.DeleteFile(ftpItem.FullName);
    }

    [FunctionName("FtpTriggerSampleFastNoFolder")]
    public static void RunFastNoFolder(
        [FtpTrigger(Connection = "FtpConnection", PollingIntervalInSeconds = 10)] FtpFile ftpItem,
        ILogger log)
    {
        log.LogInformation($"{ftpItem.GetType()} {ftpItem.Name} {ftpItem.FullName} {ftpItem.Size}  {ftpItem.Content?.Length}");
    }
}