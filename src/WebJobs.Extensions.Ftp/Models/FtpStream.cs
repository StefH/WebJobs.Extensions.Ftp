using System.IO;
using FluentFTP;

namespace WebJobs.Extensions.Ftp.Models;

public class FtpStream : FtpListItem
{
    public Stream? Stream { get; set; }
}