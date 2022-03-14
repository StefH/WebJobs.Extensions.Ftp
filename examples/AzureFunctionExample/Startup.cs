using System.IO;
using AzureFunctionExample;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureFunctionExample;

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
            .WriteTo.Console()
            .CreateLogger();
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

        builder.Services.AddFtpClient("1", config.GetSection("FtpClientOptions"));
    }
}