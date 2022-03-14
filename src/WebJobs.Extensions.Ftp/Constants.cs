using System;
using FluentFTP;
using WebJobs.Extensions.Ftp.Models;

namespace WebJobs.Extensions.Ftp;

internal static class Constants
{
    public static Type[] SupportedTypes = { typeof(FtpFile), typeof(FtpStream), typeof(FtpFile[]), typeof(FtpStream[]), typeof(IFtpClient) };

    public static Type[] SingleTypes = { typeof(FtpFile), typeof(FtpStream) };

    public static Type[] BatchTypes = { typeof(FtpFile[]), typeof(FtpStream[]) };

    public static string DefaultFtpClientName = "default";
}