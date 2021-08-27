using System.Diagnostics;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Utilities
{
    public class EventLogUtilities
    {
        public static void WriteEntry(string text, enSeverity severity)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var type = severity switch
            {
                enSeverity.Error => EventLogEntryType.Error,
                enSeverity.Warning => EventLogEntryType.Warning,
                _ => EventLogEntryType.Information
            };

            EventLog.WriteEntry("Archivist", message: text, type: type);
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }
}
