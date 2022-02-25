using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using WebJobs.Extensions.Ftp.Models;

namespace WebJobs.Extensions.Ftp.Trigger;

/// <summary>
/// Value binder class
/// </summary>
internal class FtpValueBinder : IValueBinder
{
    /// <summary>
    /// Object value
    /// </summary>
    private object _value;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="value">Object value</param>
    public FtpValueBinder(object value)
    {
        _value = value;
    }

    /// <summary>
    /// Type of object we have
    /// </summary>
    public Type Type => _value.GetType();

    /// <summary>
    /// Get value
    /// </summary>
    /// <returns>A task that contains the value</returns>
    public Task<object> GetValueAsync()
    {
        return Task.FromResult(_value);
    }

    /// <summary>
    /// Set the value
    /// </summary>
    /// <param name="value">Object value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A completed task</returns>
    public Task SetValueAsync(object value, CancellationToken cancellationToken)
    {
        _value = value;
        return Task.CompletedTask;
    }

    /// <summary>
    /// String value
    /// </summary>
    /// <returns>String value</returns>
    public string ToInvokeString()
    {
        return _value switch
        {
            FtpFile ftpFile => ftpFile.FullName,
            FtpStream ftpStream => ftpStream.FullName,
            FtpFile[] ftpFiles => $"{nameof(FtpFile)}[{ftpFiles.Length}]",
            FtpStream[] ftpStreams => $"{nameof(FtpStream)}[{ftpStreams.Length}]",

            _ => throw new InvalidCastException()
        };
    }
}