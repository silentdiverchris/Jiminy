using Jiminy.Classes;
using System.Text.Json;
using static Jiminy.Classes.Enumerations;

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
                MediaDirectoryPath = @"C:\Personal\Jiminy\Media",
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
                    new MonitoredDirectory(@"C:\Personal", recursive: true),
                    new MonitoredDirectory(@"C:\Dev", recursive: true, isActive: false)
                },
                HtmlSettings = new HtmlSettings
                {
                    HtmlTemplateFileName = @"C:\Personal\Jiminy\HtmlTemplate.html",
                    HtmlOutputFileName = @"C:\Personal\Jiminy\HtmlOutput.html"
                },
                BucketSettings = new BucketSettings
                {
                    BucketDefintions = new BucketDefinitionList
                    {
                        Buckets = new List<BucketDefinition>
                        {
                            new BucketDefinition { Name = "Incoming", DisplayOrder = 1, IconFileName = "inbox.svg", Description = "The place where new items go when they have no home"},
                            new BucketDefinition { Name = "Next", DisplayOrder = 2, IconFileName = "lightning.svg", Description = "Items to do next"},
                            new BucketDefinition { Name = "Soon", DisplayOrder = 3, Description = "Items to do soon"},
                            new BucketDefinition { Name = "Maybe", DisplayOrder = 4, Description = "Items that may or may not end up happening"},
                            new BucketDefinition { Name = "Waiting", DisplayOrder = 5, IconFileName = "hourglass.svg", Description = "Items that are waiting on other items or something else"}
                        }
                    }
                },
                TagSettings = new TagSettings
                {
                    Prefix = "=",
                    Suffix = "=",
                    Seperator = "-",
                    Delimiter = ":",
                    TagDefintions = new TagDefinitionList
                    {
                        Tags = new List<TagDefinition>
                        {
                            new TagDefinition { Type = enTagType.Repeating, Name = "Repeating", Description = "This item repeats", IconFileName = "repeating.svg" },
                            new TagDefinition { Type = enTagType.Completed, Name = "Completed", Description = "This item is completed", IconFileName = "completed.svg" },
                            new TagDefinition { Type = enTagType.Context, Name = "Context", Synonyms = new List<string>{ "ctx" }, Description = "An abstract property that sets the context of subsequent tags" },
                            new TagDefinition { Type = enTagType.Priority, Name = "Priority", Synonyms = new List<string>{ "p" }, Description = "The priority of this item", GenerateView = true, IconFileName = "priority-{value}.svg" },
                            new TagDefinition { Type = enTagType.Reminder, Name = "Reminder", Synonyms = new List<string>{ "r" }, Description = "There is a reminder for this item", GenerateView = true, IconFileName = "reminder-{value}.svg" },
                            new TagDefinition { Type = enTagType.Due, Name = "Due", Description = "There is a due date for this item", GenerateView = true, IconFileName = "due-{value}.svg" },
                            new TagDefinition { Type = enTagType.Bucket,  Name = "Bucket", Synonyms = new List<string>{ "b" }, Description = "This item is in a bucket (in, next, waiting, maybe)", GenerateView = true, IconFileName = "bucket-{value}.svg" },
                            new TagDefinition { Type = enTagType.Project, Name = "Project", Synonyms = new List<string>{ "prj" }, Description = "This item relates to a project", GenerateView = true, IconFileName = "project-{value}.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Bug", Description = "This represents a bug", GenerateView = true, IconFileName = "bug.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Conversation", Description = "This requires a conversation to be had", GenerateView = true, IconFileName = "conversation.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Phone call", Description = "This requires a phone call to be had", GenerateView = true, IconFileName = "phone.svg" }
                        }
                    }
                }
            };

            string json = JsonSerializer.Serialize(
                appSettings,
                options: new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(fileName, json);
        }

        /// <summary>
        /// Validates and does required furtling to the app settings we just loaded. The returned
        /// result determines whether we start the monitor or just bomb out with error messages
        /// </summary>
        /// <param name="appSettings"></param>
        /// <returns></returns>
        public static Result InitialiseAppSettings(AppSettings settings)
        {
            Result result = new("ValidateAppSettings", true);

            result.SubsumeResult(ValidateMonitoredDirectoryList(settings.MonitoredDirectories));

            if (!Directory.Exists(settings.MediaDirectoryPath))
            {
                result.AddError($"MediaDirectoryPath '{settings.MediaDirectoryPath}' does not exist");
            }

            if (result.HasNoErrors)
            {
                foreach (var bucket in settings.BucketSettings.BucketDefintions.Buckets.Where(_ => _.IconFileName is not null))
                {
                    bucket.IconFileName = Path.Combine(settings.MediaDirectoryPath, bucket.IconFileName!);

                    if (!File.Exists(bucket.IconFileName))
                    {
                        result.AddError($"Bucket icon file '{bucket.IconFileName}' does not exist");
                    }
                }

                foreach (var td in settings.TagSettings.TagDefintions.Tags.Where(_ => _.IconFileName is not null))
                {
                    td.IconFileName = Path.Combine(settings.MediaDirectoryPath, td.IconFileName!);

                    if (!td.IconFileName.Contains(Constants.ICON_FILE_NAME_VALUE))
                    {
                        if (!File.Exists(td.IconFileName))
                        {
                            result.AddError($"Tag icon file '{td.IconFileName}' does not exist");
                        }
                    }
                }
            }

            // Call the internal validator
            result.SubsumeResult(settings.TagSettings.ValidationResult);

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
