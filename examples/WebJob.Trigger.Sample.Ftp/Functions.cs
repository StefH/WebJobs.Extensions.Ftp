using System.IO;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Trigger;

namespace WebJob.Trigger.Sample.Ftp
{
    public class Functions
    {
        public static void RunFtpTrigger([FtpTrigger("ftp://localhost", Folder = "inbox", PollingInterval = "30s")] FtpFile ftpFile, TextWriter log)
        {
            log.WriteLine($"RunFtpTrigger(\r\n >> {ftpFile.GetType()} {ftpFile.Name} {ftpFile.FullName} {ftpFile.Size} {ftpFile.Content?.Length}");
        }
    }
}