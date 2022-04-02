namespace WebJobs.Extensions.Ftp.Enums;

public enum TriggerMode
{
    /// <summary>
    /// Trigger on the ModifyDate from the Ftp File
    /// </summary>
    ModifyDate,

    /// <summary>
    /// Trigger always (so all files present in the specified folder will be send to the trigger).
    /// </summary>
    Always
}