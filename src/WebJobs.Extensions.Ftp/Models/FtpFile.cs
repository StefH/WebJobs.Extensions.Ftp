using FluentFTP;

namespace WebJobs.Extensions.Ftp.Models;

public class FtpFile : FtpListItem
{
    public byte[]? Content { get; set; }
}