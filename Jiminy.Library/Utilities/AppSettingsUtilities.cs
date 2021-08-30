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
                RepeatSettings = new RepeatSettings
                {
                    Defintions = new RepeatDefinitionList
                    {
                        Repeats = new List<RepeatDefinition>
                        {
                            new RepeatDefinition { Name = "Daily", NumberOfDays = 1, DisplayOrder = 1, IconFileName = "repeating.svg", Colour = "red", Description = "This items repeats daily"},
                            new RepeatDefinition { Name = "Weekly", NumberOfWeeks = 1, DisplayOrder = 2, IconFileName = "repeating.svg", Colour = "green", Description = "This items repeats weekly"},
                            new RepeatDefinition { Name = "Monthly", NumberOfMonths = 1, DisplayOrder = 3, IconFileName = "repeating.svg", Colour = "green", Description = "This items repeats monthly"},
                            new RepeatDefinition { Name = "Yearly", NumberOfYears = 1, DisplayOrder = 4, IconFileName = "repeating.svg", Colour = "green", Description = "This items repeats yearly"}
                        }
                    }
                },
                BucketSettings = new BucketSettings
                {
                    Defintions = new BucketDefinitionList
                    {
                        Buckets = new List<BucketDefinition>
                        {
                            new BucketDefinition { Name = "Incoming", DisplayOrder = 1, IconFileName = "inbox.svg", Colour = "red", Description = "The place where new items go when they have no home"},
                            new BucketDefinition { Name = "Next", DisplayOrder = 2, IconFileName = "next.svg", Colour = "purple", Description = "Items to do next"},
                            new BucketDefinition { Name = "Soon", DisplayOrder = 3, IconFileName = "soon.svg", Colour = "darkgrey", Description = "Items to do soon"},
                            new BucketDefinition { Name = "Eventually", DisplayOrder = 4, Colour = "lightgrey", Description = "Items to do eventually"},
                            new BucketDefinition { Name = "Maybe", DisplayOrder = 5, Colour = "green", IconFileName = "maybe.svg", Description = "Items that may or may not end up happening"},
                            new BucketDefinition { Name = "Waiting", DisplayOrder = 6, Colour = "darkgrey", IconFileName = "waiting.svg", Description = "Items that are waiting on other items or something else"}
                        }
                    }
                },
                PrioritySettings = new PrioritySettings
                {
                    Defintions = new PriorityDefinitionList
                    {
                        Priorities = new List<PriorityDefinition>
                        {
                            new PriorityDefinition { Name = "High", Number = 1, IconFileName = "priority-high.svg", Colour = "orange"},
                            new PriorityDefinition { Name = "Medium", Number = 2, IconFileName = "priority-medium.svg", Colour = "green"},
                            new PriorityDefinition { Name = "Low", Number = 3, IconFileName = "priority-low.svg", Colour = "darkgrey"}
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
                            new TagDefinition { Type = enTagType.Completed, Name = "Completed", DisplayOrder = 10, Synonyms = new List<string>{ "closed" }, Description = "This item is completed", IconFileName = "completed.svg" },
                            new TagDefinition { Type = enTagType.Bucket, Name = "Bucket", DisplayOrder = 1, Synonyms = new List<string>{ "b" }, Description = "This item is in a bucket (in, next, waiting, maybe)", GenerateView = true, IconFileName = "bucket.svg" },
                            new TagDefinition { Type = enTagType.Priority, Name = "Priority", DisplayOrder = 2, Synonyms = new List<string>{ "p" }, Description = "The priority of this item", GenerateView = true },
                            new TagDefinition { Type = enTagType.Project, Name = "Project", DisplayOrder = 3, Synonyms = new List<string>{ "prj" }, Colour = "green", Description = "This item relates to a project", GenerateView = true, IconFileName = "project.svg" },
                            new TagDefinition { Type = enTagType.Due, Name = "Due", DisplayOrder = 4, Description = "There is a due date for this item", GenerateView = true, IconFileName = "due.svg" },
                            new TagDefinition { Type = enTagType.Reminder, Name = "Reminder", DisplayOrder = 5, Synonyms = new List<string>{ "r" }, Description = "There is a reminder for this item", GenerateView = true, IconFileName = "reminder.svg" },
                            new TagDefinition { Type = enTagType.Repeating, Name = "Repeating", DisplayOrder = 6, Description = "This item repeats", IconFileName = "repeating.svg" },
                            new TagDefinition { Type = enTagType.Context, Name = "Context", DisplayOrder = 7, Synonyms = new List<string>{ "ctx" }, Description = "An abstract property that sets the context of subsequent tags" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Bug", DisplayOrder = 8, Description = "Bug", GenerateView = true, IconFileName = "bug.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Conversation", DisplayOrder = 9, Description = "Talk to somebody", GenerateView = true, IconFileName = "conversation.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Phone call", DisplayOrder = 10, Description = "Phone call required", GenerateView = true, IconFileName = "phone.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Question", DisplayOrder = 10, Description = "Question", IconFileName = "question.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Video call", DisplayOrder = 10, Description = "Video call", IconFileName = "video-call.svg" }
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

            List<string> imageFilesToCache = new List<string> { Constants.ICON_FILE_NAME_MARKDOWN_FILE, Constants.ICON_FILE_NAME_EMBEDDED_LINK };

            if (result.HasNoErrors)
            {
                foreach (var bucket in settings.BucketSettings.Defintions.Buckets.Where(_ => _.IconFileName is not null))
                {
                    imageFilesToCache.Add(bucket.IconFileName!);
                }

                foreach (var priority in settings.PrioritySettings.Defintions.Priorities.Where(_ => _.IconFileName is not null))
                {
                    imageFilesToCache.Add(priority.IconFileName!);
                }

                foreach (var td in settings.TagSettings.TagDefintions.Tags.Where(_ => _.IconFileName is not null))
                {
                    imageFilesToCache.Add(td.IconFileName!);
                }

                foreach (var fn in imageFilesToCache.Distinct())
                {
                    if (fn.ToLower().EndsWith("svg"))
                    {
                        string ffn = Path.Combine(settings.MediaDirectoryPath, fn);

                        if (File.Exists(ffn))
                        {
                            if (!settings.SvgCache.ContainsKey(fn))
                            {
                                settings.SvgCache.Add(fn, File.ReadAllText(ffn));
                            }
                        }
                        else
                        {
                            result.AddError($"Icon file '{ffn}' does not exist");
                        }
                    }
                    else
                    {
                        result.AddError($"Icon file '{fn}' is not an svg");
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
