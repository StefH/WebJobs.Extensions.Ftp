using System;
using System.Net;
using FluentFTP;
using WebJobs.Extensions.Ftp.Options;
using WebJobs.Extensions.Ftp.Utils;

namespace WebJobs.Extensions.Ftp.Factories;

internal static class FtpClientHelper
{
    private static readonly NetworkCredential AnonymousCredentials = new("anonymous", "anonymous");

    public static IFtpClient CreateFtpClient(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Argument is null or empty", nameof(connectionString));
        }

        return CreateFtpClient(FtpUrlParser.Parse(connectionString));
    }

    public static IFtpClient CreateFtpClient(FtpClientOptions options)
    {
        return options.HasCredentials ?
            new FtpClient(options.Host, options.Port, options.Username, options.Password) :
            new FtpClient(options.Host, options.Port, AnonymousCredentials);
    }
}