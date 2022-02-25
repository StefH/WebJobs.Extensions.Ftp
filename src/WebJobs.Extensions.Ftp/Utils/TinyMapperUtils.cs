using FluentFTP;
using Nelibur.ObjectMapper;
using WebJobs.Extensions.Ftp.Models;

namespace WebJobs.Extensions.Ftp.Utils;

internal sealed class TinyMapperUtils
{
    public static TinyMapperUtils Instance { get; } = new();

    private TinyMapperUtils()
    {
        TinyMapper.Bind<FtpListItem, FtpFile>();
        TinyMapper.Bind<FtpListItem, FtpStream>();
    }

    public FtpFile MapToFtpFileItem(FtpListItem options)
    {
        return TinyMapper.Map<FtpFile>(options);
    }

    public FtpStream MapToFtpStream(FtpListItem options)
    {
        return TinyMapper.Map<FtpStream>(options);
    }
}