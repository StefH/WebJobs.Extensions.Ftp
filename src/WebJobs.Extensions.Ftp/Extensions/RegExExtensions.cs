using System.Text.RegularExpressions;

namespace WebJobs.Extensions.Ftp.Extensions;

internal static class RegExExtensions
{
    public static string? GetOptionalStringValue(this Group group)
    {
        return group.Success ? group.Value : null;
    }
}