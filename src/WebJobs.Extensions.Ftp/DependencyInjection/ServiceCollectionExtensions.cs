using System;
using Microsoft.Extensions.Configuration;
using WebJobs.Extensions.Ftp;
using WebJobs.Extensions.Ftp.Factories;
using WebJobs.Extensions.Ftp.Options;
using WebJobs.Extensions.Ftp.Utils;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFtpClient(this IServiceCollection services, string connectionString)
{
        return AddFtpClient(services, Constants.DefaultFtpClientName, connectionString);
    }

    public static IServiceCollection AddFtpClient(this IServiceCollection services, string name, string connectionString)
    {
        return AddFtpClient(services, name, FtpUrlParser.Parse(connectionString));
    }

    public static IServiceCollection AddFtpClient(this IServiceCollection services, IConfigurationSection section)
    {
        return AddFtpClient(services, Constants.DefaultFtpClientName, section);
    }

    public static IServiceCollection AddFtpClient(this IServiceCollection services, string name, IConfigurationSection section)
    {
        var options = new FtpClientOptions();
        section.Bind(options);

        return AddFtpClient(services, name, options);
    }

    public static IServiceCollection AddFtpClient(this IServiceCollection services, FtpClientOptions options)
    {
        return AddFtpClient(services, Constants.DefaultFtpClientName, options);
    }

    public static IServiceCollection AddFtpClient(this IServiceCollection services, string name, FtpClientOptions options)
    {
        return AddFtpClient(services, name,
            configure =>
            {
                configure.Host = options.Host;
                configure.Port = options.Port;
                configure.Username = options.Username;
                configure.Password = options.Password;
            });
    }

    public static IServiceCollection AddFtpClient(this IServiceCollection services, Action<FtpClientOptions> configureOptions)
    {
        return AddFtpClient(services, Constants.DefaultFtpClientName, configureOptions);
    }

    public static IServiceCollection AddFtpClient(this IServiceCollection services, string name, Action<FtpClientOptions> configureOptions)
    {
        services
            .AddSingleton<IFtpClientFactory, FtpClientFactory>()

            .AddOptions<FtpClientOptions>(name)
                .Configure(configureOptions)
                .ValidateDataAnnotations();

        return services;
    }
}
