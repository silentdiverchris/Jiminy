﻿using Jiminy.Classes;
using Jiminy.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            if (fileName.IsExistingFileName())
            {
                File.Delete(fileName);
            }

            var appSettings = new AppSettings
            {
                MediaDirectoryPath = @"C:\Personal\Jiminy\Media",
                IgnoreFileSpecifications = new List<string> { "LICENCE.*" }, // "readme.*", "README.*", 
                LogSettings = new LogSettings
                {
                    VerboseConsole = true,
                    VerboseEventLog = false,
                    LogDirectoryPath = @"C:\Personal\Jiminy\Logs",
                    SqlConnectionString = null
                },
                MonitoredDirectories = new List<MonitoredDirectory>
                {
                    new MonitoredDirectory(@"C:\Personal", recursive: true, isActive: true),
                    new MonitoredDirectory(@"C:\Work", recursive: true, isActive: true),
                    new MonitoredDirectory(@"C:\Dev\Archivist", recursive: false, isActive: true),
                    new MonitoredDirectory(@"C:\Dev\Jiminy", recursive: false, isActive: true)
                },
                OutputSettings = new OutputSettings
                {
                    ShowDiagnostics = false,
                    VerboseDiagnostics = false,
                    HtmlTemplateFileName = @"C:\Personal\Jiminy\HtmlTemplate.html",
                    Outputs = new List<OutputSpecification>
                    {
                        new OutputSpecification {
                            IsEnabled = true,
                            Title = "All Items",
                            HtmlPath = @"C:\Personal\Jiminy\Output.html",                            
                        },
                        new OutputSpecification {
                            IsEnabled = false,
                            Title = "SingLink Items",
                            HtmlPath = @"C:\Personal\Jiminy\SingLink.html",
                            ItemSelection = new ItemSelection
                            {
                                IncludeProjectNames = new List<string>
                                {
                                    "SingLink"
                                }
                            }
                        },
                        new OutputSpecification {
                            IsEnabled = false,
                            Title = "Jiminy Items",
                            HtmlPath = @"C:\Personal\Jiminy\Jiminy.html",
                            //JsonPath = @"C:\Personal\Jiminy\Jiminy.json",
                            ItemSelection = new ItemSelection
                            {
                                IncludeProjectNames = new List<string>
                                {
                                    "Jiminy"
                                }
                            }
                        },
                        new OutputSpecification {
                            IsEnabled = false,
                            Title = "Bugs and Enhancements",
                            HtmlPath = @"C:\Personal\Jiminy\BugsEnhancements.html",
                            OverrideHtmlTemplateFileName = @"C:\Personal\Jiminy\BugsEnhancementsTemplate.html",
                            ItemSelection = new ItemSelection
                            {
                                IncludeTagNames = new List<string>
                                {
                                    "Bug",
                                    "Enhancement"
                                }
                            }
                        }
                    }
                },

                // Supported colour names https://www.w3schools.com/cssref/css_colors.asp

                ProjectSettings = new ProjectSettings
                {
                    Definitions = new ProjectDefinitionList
                    {
                        Items = new List<ProjectDefinition>
                        {
                            new ProjectDefinition { Name = "Jiminy", Description = "Developing Jiminy", DisplayOrder = 103, IconFileName = "umbrella.svg", Colour = "crimson" },
                            new ProjectDefinition { Name = "Archivist", Description = "Developing Archivist", DisplayOrder = 102, IconFileName = "archivist.svg", Colour = "indigo" },
                            new ProjectDefinition { Name = "Writing", Description = "Writing English stuff", DisplayOrder = 101, IconFileName = "writing.svg", Colour = "saddlebrown" },
                            new ProjectDefinition { Name = "Hardware", DisplayOrder = 85, Description = "Hardware matters to fix or do", IconFileName = "hardware.svg", Colour = "darkblue" },
                            new ProjectDefinition { Name = "Games", DisplayOrder = 150, IconFileName = "games.svg", Colour = "purple" },
                            new ProjectDefinition { Name = "Marc", DisplayOrder = 180, Colour = "darkgrey" },
                            new ProjectDefinition { Name = "Respondent", DisplayOrder = 30, Colour = "darkblue" },
                            new ProjectDefinition { Name = "SingLink", DisplayOrder = 10, Colour = "darkblue" },
                            new ProjectDefinition { Name = "Contract", DisplayOrder = 20, Colour = "darkblue" },
                            new ProjectDefinition { Name = "Household", DisplayOrder = 90, IconFileName = "houshold.svg", Colour = "pink", Description = "Fixes and tasks at home" },
                            new ProjectDefinition { Name = "Software", DisplayOrder = 80, Colour = "orange", Description = "Software issues on home machines" },
                        }
                    }
                },
                RepeatSettings = new RepeatSettings
                {
                    Defintions = new RepeatDefinitionList
                    {
                        Items = new List<RepeatDefinition>
                        {
                            new RepeatDefinition { Name = "Daily", NumberOfDays = 1, DisplayOrder = 1, IconFileName = "repeating.svg", Colour = "red", Description = "This item repeats daily"},
                            new RepeatDefinition { Name = "Weekly", NumberOfWeeks = 1, DisplayOrder = 2, IconFileName = "repeating.svg", Colour = "green", Description = "This item repeats weekly"},
                            new RepeatDefinition { Name = "Monthly", NumberOfMonths = 1, DisplayOrder = 3, IconFileName = "repeating.svg", Colour = "green", Description = "This item repeats monthly"},
                            new RepeatDefinition { Name = "Yearly", NumberOfYears = 1, DisplayOrder = 4, IconFileName = "repeating.svg", Colour = "green", Description = "This item repeats yearly"}
                        }
                    }
                },
                BucketSettings = new BucketSettings
                {
                    Definitions = new BucketDefinitionList
                    {
                        Items = new List<BucketDefinition>
                        {
                            new BucketDefinition { Name = "Incoming", DisplayOrder = 1, IconFileName = "inbox.svg", Colour = "red", Description = "The place where new items go when they have no home"},
                            new BucketDefinition { Name = "Next", DisplayOrder = 2, IconFileName = "next.svg", Colour = "purple", Description = "Items to do next"},
                            new BucketDefinition { Name = "Soon", DisplayOrder = 3, IconFileName = "soon.svg", Colour = "blue", Description = "Items to do soon"},
                            new BucketDefinition { Name = "Waiting", DisplayOrder = 4, Colour = "darkgrey", IconFileName = "waiting.svg", Description = "Items that are waiting on other items or something else"},
                            new BucketDefinition { Name = "Maybe", DisplayOrder = 5, Colour = "green", IconFileName = "maybe.svg", Description = "Items that may or may not end up happening"},
                            new BucketDefinition { Name = "Eventually", DisplayOrder = 6, IconFileName = "eventually.svg", Colour = "darkgrey", Description = "Items to do eventually"}
                        }
                    }
                },
                PrioritySettings = new PrioritySettings
                {
                    Definitions = new PriorityDefinitionList
                    {
                        Items = new List<PriorityDefinition>
                        {
                            new PriorityDefinition { Name = "High", Number = 1, IconFileName = "priority-high.svg", Colour = "orange"},
                            new PriorityDefinition { Name = "Medium", Number = 2, IconFileName = "priority-medium.svg", Colour = "darkgrey"},
                            new PriorityDefinition { Name = "Low", Number = 3, IconFileName = "priority-low.svg", Colour = "darkgrey"}
                        }
                    }
                },
                TagSettings = new TagSettings
                {
                    Prefix = "=",
                    Suffix = "=",
                    Separator = "-",
                    Delimiter = ":",
                    Definitions = new TagDefinitionList
                    {
                        Items = new List<TagDefinition>
                        {
                            new TagDefinition { Type = enTagType.Completed, Name = "Completed", DisplayOrder = 1, Synonyms = new List<string>{ "closed", "x" }, Description = "This item is completed", IconFileName = "completed.svg" },
                            new TagDefinition { Type = enTagType.Link, Name = "Link", DisplayOrder = 2, IconFileName = "link.svg", Colour = "blue", Synonyms = new List<string>{ "url" }, Description = "A link to a URL" },
                            new TagDefinition { Type = enTagType.Bucket, Name = "Bucket", DisplayOrder = 3, Synonyms = new List<string>{ "b" }, Description = "This item is in a bucket (in, next, waiting, maybe)", GenerateView = true, IconFileName = "bucket.svg" },
                            new TagDefinition { Type = enTagType.Priority, Name = "Priority", DisplayOrder = 3, Synonyms = new List<string>{ "p" }, IconFileName = "priority-medium.svg", Description = "The priority of this item", GenerateView = true },
                            new TagDefinition { Type = enTagType.Project, Name = "Project", DisplayOrder = 3, Synonyms = new List<string>{ "prj" }, Colour = "green", Description = "This item relates to a project", GenerateView = true, IconFileName = "project.svg" },
                            new TagDefinition { Type = enTagType.Due, Name = "Due", DisplayOrder = 4, Description = "There is a due date for this item", GenerateView = true, IconFileName = "due.svg" },
                            new TagDefinition { Type = enTagType.Reminder, Name = "Reminder", DisplayOrder = 5, Synonyms = new List<string>{ "r" }, Description = "There is a reminder for this item", GenerateView = true, IconFileName = "reminder.svg" },
                            new TagDefinition { Type = enTagType.Repeating, Name = "Repeating", DisplayOrder = 6, Description = "This item repeats", IconFileName = "repeating.svg" },
                            new TagDefinition { Type = enTagType.SetContext, Name = "SetContext", DisplayOrder = 7, Synonyms = new List<string>{ "context", "ctx", "setctx" }, Description = "An abstract property that sets the context of subsequent tags" },
                            new TagDefinition { Type = enTagType.ClearContext, Name = "ClearContext", DisplayOrder = 7, Synonyms = new List<string>{ "clear", "xctx" }, Description = "An abstract property that sets the context of subsequent tags" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Bug", DisplayOrder = 8, Description = "Bug", GenerateView = true, IconFileName = "bug.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Enhancement", DisplayOrder = 8, Description = "Enhancement", GenerateView = true, IconFileName = "enhancement.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Conversation", DisplayOrder = 9, Description = "Talk to somebody", GenerateView = true, IconFileName = "conversation.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Phone Call", DisplayOrder = 10, Description = "Phone call required", GenerateView = true, IconFileName = "phone.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Question", DisplayOrder = 10, Description = "Question", IconFileName = "question.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Document", DisplayOrder = 10, Description = "Involves writing", Colour ="SlateGray", IconFileName = "document.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Video Call", DisplayOrder = 10, Description = "Video call", IconFileName = "video-call.svg" },
                            new TagDefinition { Type = enTagType.Custom, Name = "Spiked", DisplayOrder = 10, IconFileName = "spiked.svg", Description = "Spiked Online", Colour = "magenta" }
                        }
                    }
                }
            };

            string json = JsonSerializer.Serialize(
                appSettings,
                options: new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never });

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

            foreach (var ignoreSpec in settings.IgnoreFileSpecifications)
            {
                settings.IgnoredFilesRegexList.Add(ignoreSpec.GenerateRegexForFileMask());
            }

            List<string> imageFilesToCache = new List<string> { Constants.ICON_FILE_NAME_MARKDOWN_FILE, Constants.ICON_FILE_NAME_EMBEDDED_LINK };

            if (result.HasNoErrors)
            {
                // Add the default 'no project' project if it's not already defined
                
                settings.ProjectSettings.Definitions.GetOrAdd(Constants.NO_PROJECT_NAME, false, Constants.NO_PROJECT_DESCRIPTION, Constants.NO_PROJECT_ICON_FILE_NAME, Constants.NO_PROJECT_DISPLAY_ORDER);

                foreach (var bucket in settings.BucketSettings.Definitions.Items.Where(_ => _.IconFileName is not null))
                {
                    imageFilesToCache.Add(bucket.IconFileName!);
                }

                foreach (var priority in settings.PrioritySettings.Definitions.Items.Where(_ => _.IconFileName is not null))
                {
                    imageFilesToCache.Add(priority.IconFileName!);
                }

                foreach (var td in settings.TagSettings.Definitions.Items.Where(_ => _.IconFileName is not null))
                {
                    imageFilesToCache.Add(td.IconFileName!);
                }

                foreach (var td in settings.ProjectSettings.Definitions.Items.Where(_ => _.IconFileName is not null))
                {
                    imageFilesToCache.Add(td.IconFileName!);
                }

                foreach (var fn in imageFilesToCache.Distinct())
                {
                    if (fn.ToLower().EndsWith("svg"))
                    {
                        string ffn = Path.Combine(settings.MediaDirectoryPath, fn);

                        if (ffn.IsExistingFileName())
                        {
                            settings.SvgCache.Add(fn, File.ReadAllText(ffn));
                        }
                        else
                        {
                            result.AddWarning($"Icon file '{ffn}' does not exist, a default icon will be used instead");
                        }
                    }
                    else
                    {
                        result.AddWarning($"Icon file '{fn}' is not an svg file, a default icon will be used instead");
                    }
                }

                if (settings.OutputSettings.HtmlTemplateFileName.IsEmpty() || settings.OutputSettings.HtmlTemplateFileName.NoSuchFileName())
                {
                    result.AddError($"Template file '{settings.OutputSettings.HtmlTemplateFileName}' does not exist");
                }

                foreach (var tfn in settings.OutputSettings.Outputs.Where(_ => _.IsEnabled).Select(_ => _.OverrideHtmlTemplateFileName))
                {
                    if (tfn.IsExistingFileName())
                    {
                        result.AddError($"Override template file '{tfn}' does not exist");
                    }
                }
            }

            // Call the internal validator
            result.SubsumeResult(settings.TagSettings.Validate());

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

        public static Result LoadFile(string fileName, out AppSettings? appSettings)
        {
            appSettings = null;

            Result result = new("AppSettingsUtilities.LoadFile");

            if (fileName.IsExistingFileName())
            {
                string json = File.ReadAllText(fileName);

                appSettings = JsonSerializer.Deserialize<AppSettings>(json);
                
                if (appSettings is not null)
                {
                    appSettings.SettingsFileDateTime = new FileInfo(fileName).LastWriteTimeUtc;
                }
            }
            else
            {
                result.AddError($"App settings file {fileName} does not exist");
            }

            return result;
        }
    }
}
