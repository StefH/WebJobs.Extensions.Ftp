using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WebJobs.Extensions.Ftp.Factories;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Trigger;

namespace AzureFunctionExample.FtpFunctions;

public class FtpTriggerFunction
{
    private const string FunctionName = nameof(FtpTriggerFunction);

    private readonly ILogger<FtpTriggerFunction> _logger;
    private readonly IFtpClientFactory _ftpClientFactory;

    public FtpTriggerFunction(ILogger<FtpTriggerFunction> logger, IFtpClientFactory ftpClientFactory)
    {
        _logger = logger;
        _ftpClientFactory = ftpClientFactory;
    }

    [FunctionName(FunctionName)]
    public async Task RunAsync(
        [FtpTrigger("FtpConnection", Folder = "inbox", PollingInterval = "30s", IncludeContent = false)] FtpFile ftpItem)
    {
        _logger.LogInformation($"RunAsync >> {ftpItem.GetType()} {ftpItem.Name} {ftpItem.FullName} {ftpItem.Size} {ftpItem.Content?.Length}");

        var clientAnonymous = _ftpClientFactory.CreateClient("Anonymous", true);
        var stream = await clientAnonymous.OpenReadAsync(ftpItem.FullName);
        await using var mem = new MemoryStream();
        await stream.CopyToAsync(mem);
        var bytes = mem.ToArray();
        _logger.LogInformation($"clientAnonymous >> bytes={bytes.Length}");

        var clientFtp2 = _ftpClientFactory.CreateClient("Ftp2");
        await clientFtp2.CreateDirectoryAsync($"/inbox/x/{Guid.NewGuid()}");

        var client = _ftpClientFactory.CreateClient();
        await client.DeleteFileAsync(ftpItem.FullName);
    }
}