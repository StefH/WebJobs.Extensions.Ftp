using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebJobs.Extensions.Ftp.DependencyInjection;

namespace WebJob.Trigger.Sample.Ftp
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddFtp();
            });
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}