using Jiminy.Classes;
using Jiminy.Services;
using Jiminy.Utilities;
using static Jiminy.Classes.Enumerations;

namespace Jiminy
{
    public class Program
    {
#if DEBUG
        private const bool _alwaysCreateAppSettings = true;
#else
        private const bool _alwaysCreateAppSettings = false;
#endif

        static readonly object _lock = new();

        public static async Task Main(string[] args)
        {
            Result result = new("Program.Main", true);

            try
            {
                // Create default appsettings.json if it does not exist, this happens at first run, and if the
                // user deletes or moves it to generate a fresh one that they can customise.

                string appSettingsFileName = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

                WriteToConsole(new LogEntry("Jiminy.Console starting", severity: enSeverity.Success));

                if (!File.Exists(appSettingsFileName) || _alwaysCreateAppSettings)
                {
                    AppSettingsUtilities.CreateDefaultAppSettings(appSettingsFileName);
                }

                WriteToConsole(new LogEntry($"Loading settings from '{appSettingsFileName}'"));

                AppSettings? appSettings = AppSettingsUtilities.LoadAppSettings(appSettingsFileName);

                if (appSettings is not null)
                {
                    using (var monitor = new MonitorService(appSettings, new(WriteToConsole)))
                    {
                        result.SubsumeResult(await monitor.Run());
                    }
                }

                // The result has been processed within MainProcess, no need to log or display anything here
            }
            catch (Exception ex)
            {
                EventLogUtilities.WriteEntry($"Exception in Jiminy.Main {ex.Message}", enSeverity.Error);
                Console.WriteLine($"{ex.Message}");

                result.AddException(ex);
            }

            if (result.HasErrors)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }

        internal static void WriteToConsole(LogEntry entry)
        {
            if (entry.PercentComplete is not null)
            {
                lock (_lock)
                {
                    ConsoleUtilities.WriteProgressBar(
                        percentComplete: (short)entry.PercentComplete,
                        prefix: entry.ProgressPrefix,
                        suffix: entry.ProgressSuffix);
                }
            }
            else
            {
                if (entry.Severity == enSeverity.Warning)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (entry.Severity == enSeverity.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (entry.Severity == enSeverity.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine(entry.Text);

                if (entry.Severity != enSeverity.Info)
                    Console.ResetColor();
            }
        }

    }
}
