using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Stef.Validation;
using WebJobs.Extensions.Ftp.DependencyInjection;

[assembly: WebJobsStartup(typeof(FtpBinding.Startup))]
namespace WebJobs.Extensions.Ftp.DependencyInjection;

/// <summary>
/// Startup object
/// </summary>
public class FtpBinding
{
    /// <summary>
    /// IWebJobsStartup startup class
    /// </summary>
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            Guard.NotNull(builder);

            builder.AddFtp();
        }
    }
}