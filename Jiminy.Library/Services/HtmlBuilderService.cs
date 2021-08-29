using Jiminy.Classes;
using Jiminy.Helpers;
using System.Text;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Utilities
{
    internal class HtmlBuilderService : IDisposable
    {
        private readonly AppSettings _appSettings;
        private readonly TagService _tagService;

        public HtmlBuilderService(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _tagService = new TagService(appSettings);
        }

        internal async Task<Result> BuildHtmlPage(AppSettings appSettings, ItemRegistry itemRegistry, List<LogEntry> logEntries, string htmlTemplateFileName, string htmlOutputFileName)
        {
            Result result = new("BuildHtmlPage");

            // The string handling is quite heavy here, use a stringbuilder ? TODO

            if (File.Exists(htmlTemplateFileName))
            {
                string html = await File.ReadAllTextAsync(htmlTemplateFileName);
                int contentStartIdx = html.IndexOf(Constants.HTML_PLACEHOLDER_CONTENT);
                int contentEndIdx = contentStartIdx + Constants.HTML_PLACEHOLDER_CONTENT.Length;

                if (contentStartIdx == -1 || contentEndIdx == -1)
                {
                    result.AddError($"Cannot find content placeholder '{Constants.HTML_PLACEHOLDER_CONTENT}' in template file '{htmlTemplateFileName}'");
                }

                if (result.HasNoErrorsOrWarnings)
                {
                    foreach (var item in itemRegistry.Items)
                    {
                        item.AssociatedText = _tagService.ConvertURLsToLinks(item.AssociatedText, false);
                    }

                    StringBuilder sbTabHeaders = new(1000);
                    StringBuilder sbTabContent = new(1000);

                    if (itemRegistry.ReminderItems.Any)
                    {
                        // Reminders tab
                        sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Reminders", true));
                        sbTabContent.Append(GenerateRemindersTabContent(itemRegistry));
                    }

                    // Projects tab wih sub-tabs for each project
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Projects"));
                    sbTabContent.Append(GenerateProjectTabCollectionContent(itemRegistry));

                    // All buckets tab
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "All Buckets"));
                    sbTabContent.Append(GenerateBucketsTabContent(itemRegistry, "All Buckets", true));

                    // All priorities tab
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Priorities"));
                    sbTabContent.Append(GeneratePrioritiesTabContent(itemRegistry));

                    // Repeaters tab
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Repeating"));
                    sbTabContent.Append(GenerateRepeatingTabContent(itemRegistry));

                    // Admin tab wih sub-tabs
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Other"));
                    sbTabContent.Append(GenerateOtherTabCollectionContent(itemRegistry, logEntries));

                    StringBuilder sbContent = new(8000);

                    sbContent.Append(html.AsSpan(0, contentStartIdx));
                    sbContent.Append("<div class=\"container\"><div class=\"tab-wrap\">");
                    sbContent.Append(sbTabHeaders);
                    sbContent.Append(sbTabContent);
                    sbContent.Append("</div></div>");
                    sbContent.Append(html.AsSpan(contentEndIdx));

                    if (File.Exists(htmlOutputFileName))
                        File.Delete(htmlOutputFileName);

                    await File.WriteAllTextAsync(htmlOutputFileName, sbContent.ToString());
                }
                else
                {
                    result.AddError("Failed to generate new output file");
                }
            }
            else
            {
                result.AddError($"Html template file '{htmlTemplateFileName}' does not exist");
            }

            return result;
        }

        private string GenerateRemindersTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            string headerHtml = ""; // GenerateTabBodyHeader("Priorities", null, "tab-content-header");

            DateTime threshold = DateTime.UtcNow.AddDays(2);
            var imminentItems = new ItemSubSet(itemRegistry.ReminderItems.Items.Where(_ => _.ReminderDateTime < threshold));
            var futureItems = new ItemSubSet(itemRegistry.ReminderItems.Items.Where(_ => _.ReminderDateTime > threshold));

            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");
            sb.Append(GenerateListTable("Imminent", imminentItems, showBuckets: true, showPriority: true, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append(GenerateListTable("Future", futureItems, showBuckets: true, showPriority: true, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append("</div></div>");

            return sb.ToString();
        }

        //private static string GenerateReminderPanel(ItemRegistry itemRegistry)
        //{
        //    StringBuilder sb = new(200);

        //    var items = new ItemSubSet(itemRegistry.OpenItems.Items.Where(_ => _.ReminderDateTime != null).OrderBy(_ => _.ReminderDateTime));

        //    if (items.Any)
        //    {
        //        sb.Append($"<div class='reminder-panel'>");

        //        sb.Append(GenerateListTable("Reminders", items, showText: true, showPriority: true, showLinks: true, suppressProjectName: false));

        //        sb.Append("<div>");
        //    }

        //    return sb.ToString();
        //}

        private static string GenerateTabLeafHtml(string tabGroupName, string title, bool active = false)
        {
            string tabId = $"tab-{tabGroupName}-{title}".Replace(" ", "").ToLower();
            string checkedStr = active ? "checked" : "";

            string html = $"<input type=\"radio\" id=\"{tabId}\" name=\"{tabGroupName}\" class=\"tab\" {checkedStr}><label for=\"{tabId}\">{title}</label>";

            return html;
        }

        private static string GenerateTabContentHtml(string contentHtml)
        {
            string html = $"<div class=\"tab__content\">{contentHtml}</tab>";

            return html;
        }

        private string GenerateOtherTabCollectionContent(ItemRegistry itemRegistry, List<LogEntry> logEntries)
        {
            StringBuilder sb = new(2000);

            StringBuilder sbTabHeaders = new(2000);
            StringBuilder sbTabContent = new(2000);

            string headerHtml = ""; // GenerateTabBodyHeader("Other stuff", null, "tab-content-header");
            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Completed Items", true));
            sbTabContent.Append(GenerateBucketsTabContent(itemRegistry, null, false));

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Open Items"));
            sbTabContent.Append(GenerateOpenItemTabContent(itemRegistry));

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Event Log"));
            //sbTabContent.Append(GenerateTabBodyHeader("Event Log", null, "tab-content-header"));
            sbTabContent.Append(GenerateTabContentHtml(GenerateEventsTable(logEntries)));

            sb.Append(sbTabHeaders);
            sb.Append(sbTabContent);

            sb.Append("</div></div>");

            return sb.ToString();
        }

        private string GenerateProjectTabCollectionContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            StringBuilder sbTabHeaders = new(2000);
            StringBuilder sbTabContent = new(2000);

            string headerHtml = ""; // GenerateTabBodyHeader("Projects", null, "tab-content-header");
            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");

            bool activeTab = true;

            foreach (var project in itemRegistry.ProjectRegistry.Projects.OrderBy(_ => _.Priority).ThenBy(_ => _.Name))
            {
                sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_PROJECT, project.Name, activeTab));
                sbTabContent.Append(GenerateProjectTabContent(itemRegistry, project));

                activeTab = false;
            }

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_PROJECT, "All Projects", false));
            sbTabContent.Append(GenerateProjectTabContent(itemRegistry, null));

            sb.Append(sbTabHeaders);
            sb.Append(sbTabContent);

            sb.Append("</div></div>");

            return sb.ToString();
        }

        private string GenerateOpenItemTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            string headerHtml = ""; // GenerateTabBodyHeader(projectName, project is null ? null : "Project", "tab-content-header");

            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");
            sb.Append(GenerateListTable("Open Items", itemRegistry.OpenItems, showText: true, showPriority: true, showLinks: true, suppressProjectName: false));
            sb.Append("</div></div>");

            return sb.ToString();
        }

        private string GenerateProjectTabContent(ItemRegistry itemRegistry, Project? project)
        {
            string projectName = project?.Name ?? "All projects";

            var projectItems = itemRegistry.ProjectItems.Filter(onlyProjectName: project?.Name);

            StringBuilder sb = new(2000);

            string headerHtml = ""; // GenerateTabBodyHeader(projectName, project is null ? null : "Project", "tab-content-header");

            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");

            foreach (var bucket in _appSettings.BucketSettings.BucketDefintions.Buckets.OrderBy(_ => _.DisplayOrder))
            {
                sb.Append(GenerateListTable(bucket.Name, projectItems, onlyBucketName: bucket.Name, showText: true, showPriority: true, showLinks: true, suppressProjectName: project is not null));
            }

            var noBucketItems = new ItemSubSet(projectItems.Items.Where(_ => _.BucketName is null));
            sb.Append(GenerateListTable("No Bucket", noBucketItems, showText: true, showPriority: true, showLinks: true, suppressProjectName: project is not null));

            sb.Append("</div></div>");

            return sb.ToString();
        }

        private string GenerateBucketsTabContent(ItemRegistry itemRegistry, string? title, bool openItems)
        {
            StringBuilder sb = new(2000);

            var items = openItems ? itemRegistry.OpenItems : itemRegistry.CompletedItems;

            string headerHtml = ""; // GenerateTabBodyHeader(title, null, "tab-content-header");

            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");

            if (items.Any)
            {
                foreach (var bucket in _appSettings.BucketSettings.BucketDefintions.Buckets.OrderBy(_ => _.DisplayOrder))
                {
                    sb.Append(GenerateListTable(bucket.Name, items, onlyBucketName: bucket.Name, showText: true, showPriority: true, showLinks: true, suppressProjectName: false));
                }
            }
            else
            {
                sb.Append(GetNoContentHtml());
            }

            sb.Append("</div></div>");

            return sb.ToString();
        }

        private static string GetNoContentHtml()
        {
            string msg = "There are no items to view here";
            return $"<img style='margin-top:20px' src='https://i.imgflip.com/20pfsv.jpg' title='{msg}' alt='{msg}'>";
        }

        private string GeneratePrioritiesTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            string headerHtml = ""; // GenerateTabBodyHeader("Priorities", null, "tab-content-header");

            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");
            sb.Append(GenerateListTable("High", itemRegistry.OpenItems, onlyPriority: enPriority.High, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append(GenerateListTable("Medium", itemRegistry.OpenItems, onlyPriority: enPriority.Medium, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append(GenerateListTable("Low", itemRegistry.OpenItems, onlyPriority: enPriority.Low, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append("</div></div>");

            return sb.ToString();
        }

        private string GenerateRepeatingTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            string headerHtml = ""; // GenerateTabBodyHeader("Priorities", null, "tab-content-header");

            sb.Append($"<div class=\"tab__content\">{headerHtml}<div class=\"table-group\">");
            sb.Append(GenerateListTable("Daily", itemRegistry.OpenItems, onlyRepeat: enRepeat.Daily, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append(GenerateListTable("Weekly", itemRegistry.OpenItems, onlyRepeat: enRepeat.Weekly, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append(GenerateListTable("Monthly", itemRegistry.OpenItems, onlyRepeat: enRepeat.Monthly, showText: true, showLinks: true, suppressProjectName: false));
            sb.Append("</div></div>");

            return sb.ToString();
        }

        private string GenerateProjectListGroup(ItemSubSet projectItems)
        {
            var projectNames = projectItems.Items
                .OrderBy(_ => _.ProjectName)
                .Select(_ => _.ProjectName).Distinct();

            StringBuilder sb = new(1000);

            foreach (var pn in projectNames)
            {
                var itemList = new ItemSubSet(projectItems.Items.Where(_ => _.ProjectName == pn).OrderBy(_ => _.BucketName).ThenBy(_ => _.Priority));
                string? html = GenerateListTable(pn ?? "No project", itemList, showText: true, showPriority: true, showBuckets: true, showLinks: true);

                if (!string.IsNullOrEmpty(html))
                {
                    sb.Append(html);
                }
            }

            return sb.ToString();
        }

        //private static HtmlTableCell GeneratePriorityCell(Item item)
        //{
        //    string iconHtml = item.Priority switch
        //    {
        //        enPriority.High => TagSetUtilities.GetPriorityHighIconHtml(fillColour: "orangered"),
        //        enPriority.Medium => TagSetUtilities.GetPriorityMediumIconHtml(fillColour: "purple"),
        //        enPriority.Low => TagSetUtilities.GetPriorityLowIconHtml(fillColour: "darkgrey"),
        //        _ => ""
        //    };

        //    return new HtmlTableCell(iconHtml, classes: "cell-priority");
        //}

        private static HtmlTableCell GenerateBucketCell(Item tagSet)
        {
            return new HtmlTableCell(tagSet.BucketName, classes: "cell-bucket");
        }

        private static HtmlTableCell GenerateFileNameCell(Item tagSet)
        {
            return new HtmlTableCell(tagSet.FullFileName, classes: "cell-file-name");
        }

        private HtmlTableCell GenerateTextCell(Item item, bool suppressProjectName = false)
        {
            StringBuilder sbIcons = new(2000);

            string? diags = "Diagnostics: " + string.Join("<br>", item.Diagnostics);

            string? warnings = string.IsNullOrEmpty(item.Warnings)
                ? null
                : $"<br><p class='tag-warning'>{item.Warnings}</p>";

            string? project = suppressProjectName || string.IsNullOrEmpty(item.ProjectName)
                ? null
                : $"<p class='cell-project-name'>Project: {item.ProjectName}</p>";

            string reminder = "";
            string due = "";

            if (item.IsCompleted == false)
            {
                enDateStatus reminderStatus = item.GetReminderStatus(out string reminderColour);
                if (reminderStatus != enDateStatus.None)
                {
                    string iconHtml = TagService.GetReminderIconHtml(24, 24, reminderColour);

                    sbIcons.Append(TagService.CustomiseIcon(iconHtml, tipText: $"Reminder: {reminderStatus}"));
                    reminder = GenerateDateString(item.ReminderDateTime, "Reminder");
                }

                enDateStatus dueStatus = item.GetDueStatus(out string dueColour);
                if (dueStatus != enDateStatus.None)
                {
                    sbIcons.Append(TagService.CustomiseIcon(TagService.GetDueIconHtml(24, 24, dueColour), tipText: $"Due: {dueStatus}"));
                    due = GenerateDateString(item.DueDateTime, "Due");
                }
            }

            foreach (var ti in item.TagInstances.Tags.Where(_ => _.Definition.Type == enTagType.Custom))
            {
                sbIcons.Append(_tagService.GetTagIconHtml(ti));
            }

            string? text = string.IsNullOrEmpty(item.AssociatedText)
                ? item.IsContext
                    ? "[Context item]"
                    : null
                : $"<p class='cell-text'>{item.AssociatedText}</p>";

            //if (item.BucketName is not null)
            //{
            //    //string bucketIcon = item.BucketName switch
            //    //{
            //    //    enBucket.In => TagService.CustomiseIcon(TagService.GetInBucketHtml(fillColour: "orangered"), tipText: $"Bucket: {item.Bucket}"),
            //    //    enBucket.Next => TagService.CustomiseIcon(TagService.GetNextBucketHtml(fillColour: "orangered"), tipText: $"Bucket: {item.Bucket}"),
            //    //    enBucket.Waiting => TagService.CustomiseIcon(TagService.GetWaitingBucketHtml(fillColour: "orangered"), $"Bucket: {item.Bucket}"),
            //    //    enBucket.Maybe => TagService.CustomiseIcon(TagService.GetMaybeBucketHtml(fillColour: "orangered"), tipText: $"Bucket: {item.Bucket}"),
            //    //    _ => ""
            //    //};

            //    sbIcons.Append(gen);
            //}

            string priorityIcon = item.Priority switch
            {
                enPriority.High => TagService.CustomiseIcon(TagService.GetPriorityHighIconHtml(fillColour: "orangered"), tipText: $"Priority: {item.Priority}"),
                enPriority.Medium => TagService.CustomiseIcon(TagService.GetPriorityMediumIconHtml(fillColour: "purple"), tipText:  $"Priority: {item.Priority}"),
                enPriority.Low => TagService.CustomiseIcon(TagService.GetPriorityLowIconHtml(fillColour: "darkgrey"), tipText: $"Priority: {item.Priority}"),
                _ => ""
            };
            sbIcons.Append(priorityIcon);

            sbIcons.Append(TagService.CustomiseIcon(TagService.GetLinkIconHtml(), tipText: "Open the source MarkDown file", linkUrl: item.FullFileName));

            foreach (var ti in item.TagInstances.Tags)
            {
                sbIcons.Append(_tagService.GetTagIconHtml(ti));
            }

            return new HtmlTableCell(
                $"{project}{text}{reminder}{due}{warnings}<div class='item-icons'>{sbIcons}</div>"); // title: diags);
        }

        private static string GenerateDateString(DateTime? dateTime, string? prefix)
        {
            string dateStr = "";

            if (prefix is not null)
            {
                prefix += " ";
            }

            if (dateTime is not null)
            {
                DateTime dt = (DateTime)dateTime;

                string dtStr = FormatReminderDate(dt);
                string colour = dateTime.DateColour();

                if (dt.Date == DateTime.Now.Date)
                {
                    dateStr = $"<p class='reminder-date-today' style='color:{colour}'>{prefix}Today</p>";
                }
                else if (dt > DateTime.Now.AddDays(2))
                {
                    dateStr = $"<p class='reminder-date-future' style='color:{colour}'>{prefix}{dtStr}</p>";
                }
                else if (dt.Date > DateTime.Now.Date)
                {
                    dateStr = $"<p class='reminder-date-soon' style='color:{colour}'>{prefix}{dtStr}</p>";
                }
                else
                {
                    dateStr = $"<p class='reminder-date-past' style='color:{colour}'>{prefix}{dtStr}</p>";
                }
            }

            return dateStr;
        }

        private static string FormatReminderDate(DateTime dt)
        {
            string? dtStr;

            if (dt.Date == DateTime.Now.Date)
            {
                dtStr = "today";
            }
            else
            {
                if (dt.Hour == 0 && dt.Minute == 0)
                {
                    dtStr = dt.ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY);
                }
                else
                {
                    dtStr = dt.ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_TIME);
                }

                int daysDiff = (int)(dt - DateTime.Now.Date).TotalDays;

                if (daysDiff > 0)
                {
                    if (daysDiff == 1)
                    {
                        dtStr = " tomorrow";
                    }
                    else
                    {
                        dtStr += $", in {daysDiff} day{daysDiff.PluralSuffix()}";
                    }
                }
                else
                {
                    daysDiff = -1 * daysDiff;

                    if (daysDiff == 1)
                    {
                        dtStr = "yesterday";
                    }
                    else
                    {
                        dtStr += $", {daysDiff} day{daysDiff.PluralSuffix()} ago";
                    }
                }
            }

            return dtStr;
        }

        //private static HtmlTableCell GenerateLinksCell(Item tagSet)
        //{
        //    string iconHtml = TagSetUtilities.GetLinkIconHtml();

        //    return new HtmlTableCell(
        //        text: iconHtml,
        //        linkUrl: tagSet.FullFileName, styles: "text-align:center", title: $"{tagSet.FullFileName} line {tagSet.LineNumber} original {tagSet.RawTagSet}",
        //        classes: "cell-links");
        //}

        private static string GenerateEventsTable(List<LogEntry> eventList)
        {
            StringBuilder sb = new(1000);

            sb.Append("<table class='event-log-table'><tr><th>Time</th><th>Event</th></tr>");

            foreach (var ev in eventList)
            {
                sb.Append($"<tr><td>{ev.CreatedUtc.ToString(Constants.DATE_FORMAT_TIME_ONLY_SECONDS_FRACTION)}</td><td>{ev.Text}</td></tr>");
            }

            sb.Append("</table>");

            return sb.ToString();
        }

        private string? GenerateListTable(
            string title,
            ItemSubSet itemSubset,
            int displayOrder = 0,
            bool showText = false,
            bool showPriority = false,
            bool showLinks = false,
            bool showBuckets = false,
            bool showFileName = false,
            enPriority? onlyPriority = null,
            string? onlyBucketName = null,
            enRepeat? onlyRepeat = null,
            string? onlyProjectName = null,
            bool suppressProjectName = false)
        {
            if (itemSubset.Any)
            {
                var filtered = itemSubset.Filter(onlyProjectName, onlyPriority, onlyBucketName, onlyRepeat);

                HtmlTable table = new() { DisplayOrder = displayOrder };

                var headerCells = new List<HtmlTableCell>();

                if (showText)
                    headerCells.Add(new HtmlTableCell("Text", classes: "cell-text", isHeader: true));

                //if (showPriority)
                //    headerCells.Add(new HtmlTableCell("Priority", classes: "cell-priority", isHeader: true));

                if (showBuckets)
                    headerCells.Add(new HtmlTableCell("Bucket", classes: "cell-bucket", isHeader: true));

                if (showFileName)
                    headerCells.Add(new HtmlTableCell("File", classes: "cell-file-name", isHeader: true));

                //if (showLinks)
                //    headerCells.Add(new HtmlTableCell("Links", classes: "cell-links", isHeader: true));

                table.Rows.Add(new()
                {
                    IsHeader = true,
                    Cells = headerCells
                });

                if (filtered.Any)
                {
                    foreach (var item in filtered.Items)
                    {
                        var bodyCells = new List<HtmlTableCell>();

                        if (showText)
                            bodyCells.Add(GenerateTextCell(item, suppressProjectName));

                        //if (showPriority)
                        //    bodyCells.Add(GeneratePriorityCell(item));

                        if (showBuckets)
                            bodyCells.Add(GenerateBucketCell(item));

                        if (showFileName)
                            bodyCells.Add(GenerateFileNameCell(item));

                        //if (showLinks)
                        //    bodyCells.Add(GenerateLinksCell(item));

                        HtmlTableRow row = new()
                        {
                            IsHeader = false,
                            Cells = bodyCells
                        };

                        table.Rows.Add(row);
                    }

                    return GenerateTableElement(title, table.GeneratedHtml);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static string GenerateTableElement(string title, string tableHtml)
        {
            return $"<div class='table-item'><div class='table-title'>{title}</div>{tableHtml}</div>";
        }

        private static string GenerateTabBodyHeader(string title, string? titlePrefix, string headerClass)
        {
            string? prefixHtml = titlePrefix is null
                ? null
                : $"<div class='prefix'>{titlePrefix}</div>";

            return $"<div class='{headerClass}'>{prefixHtml}<div class='name'>{title}</div></div>";
        }

        private static string GenerateTab(string title, string contentHtml)
        {
            string tabId = title.ToLower();
            string titleHtml = GenerateTabBodyHeader(title, null, "tab-header");
            string tabHtml = $"<div class=\"tab\" id=\"{tabId}\"><a href = \"#{tabId}\">{titleHtml}</a><div class=\"content\">{contentHtml}</div></div>";

            return tabHtml;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HtmlBuilderService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private bool disposedValue;
    }
}
