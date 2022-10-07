using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Bindings;
using WebJobs.Extensions.Ftp.Factories;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Trigger;

namespace AzureFunctionFtpExample.FtpFunctions;

public class FtpTriggerFunction
{
    private const string FunctionName = nameof(FtpTriggerFunction);
    private const string FtpConnection = "FtpConnection";
    private const string Folder = "inbox";

    private readonly ILogger<FtpTriggerFunction> _logger;
    private readonly IFtpClientFactory _ftpClientFactory;

    public FtpTriggerFunction(ILogger<FtpTriggerFunction> logger, IFtpClientFactory ftpClientFactory)
    {
        _logger = logger;
        _ftpClientFactory = ftpClientFactory;
    }

    [Singleton]
    [FunctionName(FunctionName)]
    public async Task RunAsync(
        [FtpTrigger(FtpConnection, Folder = Folder, PollingInterval = "30s", IncludeContent = false)] FtpFile ftpFile,
        [Ftp(FtpConnection, Folder = Folder, AutoConnectFtpClient = true)] IFtpClient client)
    {
        _logger.LogInformation($"RunAsync >> {ftpFile.GetType()} {ftpFile.Name} {ftpFile.FullName} {ftpFile.Size} {ftpFile.Content?.Length}");

        _logger.LogInformation("IFtpClient IsConnected = {connected}", client.IsConnected);

        await client.DeleteFileAsync(ftpFile.FullName);



        //var clientAnonymous = _ftpClientFactory.CreateClient("Anonymous", true);
        //var stream = await clientAnonymous.OpenReadAsync(ftpItem.FullName);
        //await using var mem = new MemoryStream();
        //await stream.CopyToAsync(mem);
        //var bytes = mem.ToArray();
        //_logger.LogInformation($"clientAnonymous >> bytes={bytes.Length}");

        //var clientFtp2 = _ftpClientFactory.CreateClient("Ftp2");
        //await clientFtp2.CreateDirectoryAsync($"/inbox/x/{Guid.NewGuid()}");

        //var client = _ftpClientFactory.CreateClient();
        //await client.DeleteFileAsync(ftpItem.FullName);
    }
}