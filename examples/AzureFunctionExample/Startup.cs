using System.IO;
using AzureFunctionFtpExample;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureFunctionFtpExample;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", true, false)
            .AddEnvironmentVariables()
            .Build();

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(config)
            .MinimumLevel.Override("AzureFunctionFtpExample", LogEventLevel.Information)
            .MinimumLevel.Override("WebJobs.Extensions.Ftp", LogEventLevel.Information)
            .WriteTo.Console()
            .CreateLogger();
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

        builder.Services.AddFtpClient("Ftp2", config.GetConnectionString("Ftp2"));
        builder.Services.AddFtpClient(config.GetSection("FtpClientOptions"));
        builder.Services.AddFtpClient("Anonymous", config.GetSection("FtpClientOptionsAnonymous"));
    }
}