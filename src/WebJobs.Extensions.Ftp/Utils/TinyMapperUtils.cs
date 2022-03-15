using FluentFTP;
using Nelibur.ObjectMapper;
using WebJobs.Extensions.Ftp.Models;
using WebJobs.Extensions.Ftp.Options;

namespace WebJobs.Extensions.Ftp.Utils;

internal sealed class TinyMapperUtils
{
    public static TinyMapperUtils Instance { get; } = new();

    private TinyMapperUtils()
    {
        TinyMapper.Bind<FtpListItem, FtpFile>();
        TinyMapper.Bind<FtpListItem, FtpStream>();
        TinyMapper.Bind<FtpClientOptions, FtpClientOptions>();
    }

    public FtpFile MapToFtpFileItem(FtpListItem ftpListItem)
    {
        return TinyMapper.Map<FtpFile>(ftpListItem);
    }

    public FtpStream MapToFtpStream(FtpListItem ftpListItem)
    {
        return TinyMapper.Map<FtpStream>(ftpListItem);
    }

    public void Map(FtpClientOptions source, FtpClientOptions target)
    {
        TinyMapper.Map(source, target);
    }
}