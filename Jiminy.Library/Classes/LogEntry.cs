using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    public class LogEntry
    {
        public LogEntry()
        {
            CreatedUtc = DateTime.UtcNow;
        }

        public LogEntry(short percentComplete, string suffix, string prefix)
        {
            ProgressPrefix = prefix;
            ProgressSuffix = suffix;
            PercentComplete = percentComplete;
        }

        public LogEntry(string logText, enSeverity severity = enSeverity.Info, DateTime? createdUtc = null, bool alwaysWriteToEventLog = false)
        {
            Text = logText;
            Severity = severity;
            CreatedUtc = createdUtc ?? DateTime.UtcNow;
            AlwaysWriteToEventLog = alwaysWriteToEventLog;
        }

        public string? ProgressPrefix { get; set; } = null;
        public string? ProgressSuffix { get; set; } = null;
        public short? PercentComplete { get; set; } = null;

        public bool AlwaysWriteToEventLog { get; set; }
        public DateTime CreatedUtc { get; private set; }
        public enSeverity Severity { get; set; }
        public string? Text { get; set; }

        public string FormatForFile()
        {
            return $"{CreatedUtc:HH:mm:ss} {Severity.ToString().PadRight(7)} {Text}\r\n";
        }
    }
}
