namespace Jiminy.Classes
{
    public class LogSettings
    {
        public bool VerboseConsole { get; set; } = false;
        public bool VerboseEventLog { get; set; } = false;

        public string? LogDirectoryPath { get; set; } = "Log";
        public string? SqlConnectionString { get; set; } = null;
    }
}
