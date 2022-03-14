using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using WebJobs.Extensions.Ftp.Factories;

namespace WebJobs.Extensions.Ftp.DependencyInjection;

/// <summary>
/// WebJobBuilder extension to add FTP extensions
/// </summary>
public static class FtpWebJobsBuilderExtensions
{
    /// <summary>
    /// Extension method to add our custom extensions
    /// </summary>
    /// <param name="builder"><c>IWebJobsBuilder</c> instance</param>
    /// <returns><c>IWebJobsBuilder</c> instance</returns>
    /// <exception>Throws ArgumentNullException if builder is null</exception>
    public static IWebJobsBuilder AddFtp(this IWebJobsBuilder builder)
    {
        builder.AddExtension<FtpExtensionConfigProvider>();

        builder.Services.AddSingleton<IFtpClientFactory, FtpClientFactory>();
        builder.Services.AddLogging();

        return builder;
    }
}