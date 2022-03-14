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
        [FtpTrigger("FtpConnection", Folder = "inbox", PollingInterval = "3s", IncludeContent = false)] FtpFile ftpItem)
    {
        _logger.LogWarning($"RunAsync >> {ftpItem.GetType()} {ftpItem.Name} {ftpItem.FullName} {ftpItem.Size} {ftpItem.Content?.Length}");

        var client = _ftpClientFactory.CreateClient("1");
        await client.DeleteFileAsync(ftpItem.FullName);
    }
}