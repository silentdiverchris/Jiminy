using Jiminy.Classes;
using Jiminy.Services;
using Jiminy.Utilities;
using System;
using System.Threading.Tasks;
using static Jiminy.Classes.Enumerations;

namespace Jiminy
{
    public class Program
    {
        static readonly object _lock = new();

        public static async Task Main(string[] args)
        {
            Result result = new("Program.Main", true);

            try
            {
                WriteToConsole(new LogEntry("Jiminy.Console starting", severity: enSeverity.Success));

                using (var monitor = new MonitorService(new(WriteToConsole)))
                {
                    if (monitor.IsInitialised)
                    {
                        result.SubsumeResult(await monitor.Run());
                    }
                    else
                    {
                        result.AddError("Monitor failed to initialise");
                    }
                }

                WriteToConsole(new LogEntry(result.TextSummary, severity: result.HasErrors ? enSeverity.Error : result.HasWarnings ? enSeverity.Warning : enSeverity.Info));
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
