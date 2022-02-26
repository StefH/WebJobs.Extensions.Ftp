using System;
using System.Text.RegularExpressions;
using WebJobs.Extensions.Ftp.Trigger;

namespace WebJobs.Extensions.Ftp.Utils;

internal static class PollingIntervalParser
{
    private static readonly TimeSpan DefaultTimeSpan = new(0, 1, 0);
    private static readonly Regex RegEx = new("^(?<number>\\d+)(?<unit>[s|m|h|d])$");

    public static TimeSpan Parse(string? value)
    {
        if (value == null)
        {
            return DefaultTimeSpan;
        }

        var match = RegEx.Match(value);
        if (!match.Success || !int.TryParse(match.Groups["number"].Value, out var number))
        {
            throw new ArgumentException($"The PollingInterval with value '{value}' is not valid. It should be in the form of '{RegEx}'. For example use \"10s\" for 10 seconds.", nameof(FtpTriggerAttribute.PollingInterval));
        }

        return match.Groups["unit"].Value[0] switch
        {
            's' => TimeSpan.FromSeconds(number),
            'm' => TimeSpan.FromMinutes(number),
            'h' => TimeSpan.FromHours(number),
            'd' => TimeSpan.FromDays(number),
            _ => DefaultTimeSpan
        };
    }
}