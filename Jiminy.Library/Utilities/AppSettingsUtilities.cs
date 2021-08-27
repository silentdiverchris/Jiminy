using Jiminy.Classes;
using System.Text.Json;

namespace Jiminy.Utilities
{
    public class AppSettingsUtilities
    {
        /// <summary>
        /// Creates a default, and invalid by design appsettings file that must be 
        /// customised. This will be created on first run (or if the file does not exist), the
        /// errors will be reported and the program will terminate without doing anything.
        /// </summary>
        /// <param name="fileName"></param>
        public static void CreateDefaultAppSettings(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var appSettings = new AppSettings
            {
                IgnoreFileSpecifications = new List<string> { "readme.*", "README.*", "LICENCE.*" },
                LogSettings = new LogSettings
                {
                    VerboseConsole = true,
                    VerboseEventLog = false,
                    LogDirectoryPath = @"C:\Personal\Jiminy\Logs",
                    SqlConnectionString = null
                },
                MonitoredDirectories = new List<MonitoredDirectory>
                {
                    new MonitoredDirectory(@"C:\Personal", true),
                    new MonitoredDirectory(@"D:\Temp\Jiminy\Dir1", false),
                    new MonitoredDirectory(@"D:\Temp\Jiminy\Dir2", false)
                },
                MarkdownSettings = new MarkdownSettings
                {
                    Test = "test",
                    TagDelimiterSets = new List<ItemDelimiterSet>
                    {
                        new ItemDelimiterSet("@", " ", "@", ":" )
                    }
                }
            };

            string json = JsonSerializer.Serialize(
                appSettings,
                options: new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(fileName, json);
        }

        public static Result ValidateAppSettings(AppSettings appSettings)
        {
            Result result = new("ValidateAppSettings", true);

            result.SubsumeResult(ValidateMonitoredDirectoryList(appSettings.MonitoredDirectories));

            return result;
        }

        private static Result ValidateMonitoredDirectoryList(List<MonitoredDirectory> dirList)
        {
            Result result = new("ValidateMonitoredDirectoryList", true);

            if (dirList.Any())
            {
                foreach (var dir in dirList)
                {
                    if (Directory.Exists(dir.Path))
                    {
                        dir.Exists = true;
                    }
                    else
                    {
                        dir.Exists = false;
                        result.AddError($"Monitored directory '{dir.Path}' does not exist, it will not be monitored");
                    }
                }
            }
            else
            {
                result.AddError($"No monitored directories have been defined");
            }

            return result;
        }

        public static AppSettings? LoadAppSettings(string fileName)
        {
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);

                var appSettings = JsonSerializer.Deserialize<AppSettings>(json);

                return appSettings;
            }
            else
            {
                throw new Exception($"App settings file {fileName} does not exist");
            }
        }
    }
}
