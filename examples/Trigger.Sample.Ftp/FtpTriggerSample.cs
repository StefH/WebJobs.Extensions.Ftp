using System;
using FluentFTP;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Bindings;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Trigger;

namespace Trigger.Sample.Ftp;

public static class FtpTriggerSample
{
    private const string FtpConnection = "FtpConnection";
    private const string Folder = "inbox";

    [FunctionName("FtpTriggerFtpFile")]
    public static void RunFtpTriggerFtpFile(


        [FtpTrigger("FtpConnectionAnonymous", Folder = "inbox", PollingInterval = "30s")] FtpFile ftpFile,

        ILogger log)
    {
        log.LogInformation($"{nameof(RunFtpTriggerFtpFile)} >> {ftpFile.GetType()} {ftpFile.Name} {ftpFile.FullName} {ftpFile.Size} {ftpFile.Content?.Length}");
    }

    [FunctionName("FtpTriggerFtpFiles")]
    public static void RunFtpTriggerFtpFiles(


        [FtpTrigger(Connection = FtpConnection, Folder = Folder, PollingInterval = "10s", BatchSize = 3)] FtpFile[] ftpFiles,

        ILogger log)
    {
        log.LogWarning($"{nameof(RunFtpTriggerFtpFiles)} >> {ftpFiles.GetType()} {ftpFiles.Length}");
    }

    [FunctionName("FtpTriggerFtpStreams")]
    public static void RunFtpTriggerFtpStreams(


        [FtpTrigger(Connection = FtpConnection, Folder = Folder, PollingInterval = "10s")] FtpStream[] ftpStreams,

        ILogger log)
    {
        log.LogWarning($"{nameof(RunFtpTriggerFtpStreams)} >> {ftpStreams.GetType()} {ftpStreams.Length}");
    }

    //[FunctionName("FtpTriggerFtpStream")]
    //public static async Task RunFtpTriggerFtpStream(
    //    [FtpTrigger(Connection = FtpConnection, Folder = Folder, PollingInterval = "10s")] FtpStream ftpStream,
    //    ILogger log)
    //{
    //    log.LogInformation($"FtpTriggerFtpStream >> {ftpStream.GetType()} {ftpStream.Name} {ftpStream.FullName} {ftpStream.Size} {ftpStream.Stream?.Length}");

    //    if (ftpStream.Stream != null)
    //    {
    //        log.LogInformation($"FtpTriggerFtpStream >> CanRead={ftpStream.Stream.CanRead}");

    //        await using var mem = new MemoryStream();
    //        await ftpStream.Stream.CopyToAsync(mem);

    //        var bytes = mem.ToArray();
    //        log.LogInformation($"FtpTriggerFtpStream >> bytes={bytes.Length}");
    //    }
    //}

    [FunctionName("FtpTriggerSampleWithClient")]
    public static void RunFtpTriggerSampleWithClient(
        [FtpTrigger(Connection = FtpConnection, Folder = Folder, PollingInterval = "30s", IncludeContent = false, TriggerOnStartup = true)] FtpFile ftpFile,
        [Ftp(Connection = FtpConnection, Folder = Folder)] IFtpClient client,
        ILogger log)
    {
        log.LogInformation($"FtpTriggerSampleWithClient >> {ftpFile.GetType()} {ftpFile.Name} {ftpFile.FullName} {ftpFile.Size} {ftpFile.Content?.Length}");

        // Do some processing with the FtpFile and client

        log.LogInformation("FtpTriggerSampleWithClient >> IsConnected {IsConnected}", client.IsConnected);
        if (!client.IsConnected)
        {
            client.Connect();
        }

        try
        {
            client.DeleteFile(ftpFile.FullName);
        }
        catch (Exception e)
        {
            log.LogError(e, "DeleteFile");
        }

        //client.Disconnect();
        //client.Dispose();
    }

    //[FunctionName("FtpTriggerSampleFastNoFolder")]
    //public static void RunFastNoFolder(
    //    [FtpTrigger(Connection = "FtpConnection", PollingInterval = "10s")] FtpFile ftpFile,
    //    ILogger log)
    //{
    //    log.LogInformation($"{ftpFile.GetType()} {ftpFile.Name} {ftpFile.FullName} {ftpFile.Size}  {ftpFile.Content?.Length}");
    //}
}