using Jiminy.Classes;
using Jiminy.Helpers;
using System.Text;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Utilities
{
    internal static class HtmlBuilderUtilities
    {
        internal static async Task<Result> BuildHtmlPage(AppSettings appSettings, ItemRegistry itemRegistry, List<LogEntry> logEntries)
        {
            Result result = new("BuildHtmlPage");

            string htmlTemplateFileName = appSettings.HtmlSettings.HtmlTemplateFileName.Contains(Path.DirectorySeparatorChar)
                ? appSettings.HtmlSettings.HtmlTemplateFileName
                : Path.Join(AppDomain.CurrentDomain.BaseDirectory, appSettings.HtmlSettings.HtmlTemplateFileName);

            string htmlOutputFileName = appSettings.HtmlSettings.HtmlOutputFileName.Contains(Path.DirectorySeparatorChar)
                ? appSettings.HtmlSettings.HtmlOutputFileName
                : Path.Join(AppDomain.CurrentDomain.BaseDirectory, appSettings.HtmlSettings.HtmlOutputFileName);

            // The string handling is quite heavy here, use a stringbuilder ? TODO

            if (File.Exists(htmlTemplateFileName))
            {
                string html = await File.ReadAllTextAsync(htmlTemplateFileName);

                string? generatedHtml = null;

                //if (html.Contains(Constants.HTML_PLACEHOLDER_ALL_ITEMS))
                //{
                //    generatedHtml = GenerateListTable("All items", itemRegistry.Items.OrderBy(_ => _.AssociatedText), showText: true, showBuckets: true, showLinks: true, showFileName: true, showPriority: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_ALL_ITEMS, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_PRIORITY_1_LIST))
                //{
                //    generatedHtml = GenerateListTable("High Priority", itemRegistry.OpenItems, onlyPriority: enPriority.High, showText: true, showBuckets: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_PRIORITY_1_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_PRIORITY_2_LIST))
                //{
                //    generatedHtml = GenerateListTable("Medium Priority", itemRegistry.OpenItems, onlyPriority: enPriority.Medium, showText: true, showBuckets: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_PRIORITY_2_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_PRIORITY_3_LIST))
                //{
                //    generatedHtml = GenerateListTable("Low Priority", itemRegistry.OpenItems, onlyPriority: enPriority.Low, showText: true, showBuckets: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_PRIORITY_3_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_DAILY_LIST))
                //{
                //    generatedHtml = GenerateListTable("Daily", itemRegistry.OpenItems, onlyDaily: true, showText: true, showPriority: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_DAILY_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_WEEKLY_LIST))
                //{
                //    generatedHtml = GenerateListTable("Weekly", itemRegistry.OpenItems, onlyWeekly: true, showText: true, showPriority: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_WEEKLY_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_MONTHLY_LIST))
                //{
                //    generatedHtml = GenerateListTable("Monthly", itemRegistry.OpenItems, onlyMonthly: true, showText: true, showPriority: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_MONTHLY_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_COMPLETED_LIST))
                //{
                //    generatedHtml = GenerateListTable("Completed", itemRegistry.CompletedItems, showText: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_COMPLETED_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_BUCKET_IN_LIST))
                //{
                //    generatedHtml = GenerateListTable("Incoming", itemRegistry.BucketItems, onlyBucket: enBucket.In, showText: true, showPriority: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_BUCKET_IN_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_BUCKET_NEXT_LIST))
                //{
                //    generatedHtml = GenerateListTable("Next", itemRegistry.BucketItems, onlyBucket: enBucket.Next, showText: true, showPriority: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_BUCKET_NEXT_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_BUCKET_WAIT_LIST))
                //{
                //    generatedHtml = GenerateListTable("Waiting", itemRegistry.BucketItems, onlyBucket: enBucket.Waiting, showText: true, showPriority: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_BUCKET_WAIT_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_BUCKET_SOMEDAY_LIST))
                //{
                //    generatedHtml = GenerateListTable("Someday/maybe", itemRegistry.BucketItems, onlyBucket: enBucket.Someday, showText: true, showPriority: true, showLinks: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_BUCKET_SOMEDAY_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_FOUND_IN_FILES_LIST))
                //{
                //    var foundIFilesTags = itemRegistry.Select(_ => _.FullFileName).Distinct().OrderBy(_ => _).Select(_ => new Item { FullFileName = _ });
                //    generatedHtml = GenerateListTable("Items found in these files", foundIFilesTags, showFileName: true);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_FOUND_IN_FILES_LIST, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_PROJECT_LIST_GROUP))
                //{
                //    generatedHtml = GenerateProjectListGroup(itemRegistry.ProjectItems);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_PROJECT_LIST_GROUP, generatedHtml);
                //}

                //if (html.Contains(Constants.HTML_PLACEHOLDER_EVENT_LOG))
                //{
                //    generatedHtml = GenerateEventsTable(logEntries);
                //    html = html.Replace(Constants.HTML_PLACEHOLDER_EVENT_LOG, generatedHtml);
                //}

                if (html.Contains(Constants.HTML_PLACEHOLDER_PROJECTS_TABS))
                {
                    StringBuilder sbTabHtml = new(2000);

                    foreach (var project in itemRegistry.ProjectRegistry.Projects.OrderBy(_ => _.Priority))
                    {
                        string projectTabHtml = GenerateProjectTabHtml(itemRegistry, project);
                        sbTabHtml.Append(projectTabHtml);
                    }

                    string allProjectsTabHtml = GenerateProjectTabHtml(itemRegistry, null);
                    sbTabHtml.Append(allProjectsTabHtml);

                    html = html.Replace(Constants.HTML_PLACEHOLDER_PROJECTS_TABS, sbTabHtml.ToString());
                }

                if (html.Contains(Constants.HTML_PLACEHOLDER_ALL_PRIORITIES_TAB))
                {
                    StringBuilder sbTabHtml = new(2000);

                    StringBuilder sbLists = new(2000);
                    
                    string headerHtml = GenerateHeader("Priorities", null, "project-header");
                    sbLists.Append($"{headerHtml}<div class=\"table-group\">");

                    foreach (var pri in Enum.GetValues(typeof(enPriority)))
                    {
                        int priVal = (int)pri;
                        enPriority thisPriority = (enPriority)priVal;

                        sbLists.Append(GenerateListTable($"Priority {thisPriority}", itemRegistry.OpenItems, onlyPriority: thisPriority, showText: true, showPriority: false, showLinks: true));
                    }

                    sbLists.Append("</div>");

                    sbTabHtml.Append(GenerateTab("Priorities", null, sbLists.ToString()));

                    html = html.Replace(Constants.HTML_PLACEHOLDER_ALL_PRIORITIES_TAB, sbTabHtml.ToString());
                }

                if (File.Exists(htmlOutputFileName))
                    File.Delete(htmlOutputFileName);

                await File.WriteAllTextAsync(htmlOutputFileName, html);
            }
            else
            {
                result.AddError($"Html template file '{htmlTemplateFileName}' does not exist");
            }

            return result;
        }

        private static string GenerateProjectTabHtml(ItemRegistry itemRegistry, Project? project)
        {
            string projectName = project?.Name ?? "All projects";

            var projectItems = itemRegistry.ProjectItems.Filter(onlyProjectName: project?.Name);

            StringBuilder sbLists = new(2000);

            string headerHtml = GenerateHeader(projectName, "Project", "project-header");

            sbLists.Append($"{headerHtml}<div class=\"table-group\">");
            sbLists.Append(GenerateListTable("Incoming", projectItems, onlyBucket: enBucket.In, showText: true, showPriority: true, showLinks: true, suppressProjectName: true));
            sbLists.Append(GenerateListTable("Next", projectItems, onlyBucket: enBucket.Next, showText: true, showPriority: true, showLinks: true, suppressProjectName: true));
            sbLists.Append(GenerateListTable("Waiting", projectItems, onlyBucket: enBucket.Waiting, showText: true, showPriority: true, showLinks: true, suppressProjectName: true));
            sbLists.Append(GenerateListTable("Someday/maybe", projectItems, onlyBucket: enBucket.Someday, showText: true, showPriority: true, showLinks: true, suppressProjectName: true));
            sbLists.Append("</div>");

            string projectTabHtml = GenerateTab(projectName, project is null ? null : "Project", sbLists.ToString());

            return projectTabHtml;
        }

        private static string GenerateProjectListGroup(ItemSubSet projectItems)
        {
            var projectNames = projectItems.Items
                .OrderBy(_ => _.ProjectName)
                .Select(_ => _.ProjectName).Distinct();

            StringBuilder sb = new(1000);

            foreach (var pn in projectNames)
            {
                var itemList = new ItemSubSet(projectItems.Items.Where(_ => _.ProjectName == pn).OrderBy(_ => _.Bucket).ThenBy(_ => _.Priority));
                string? html = GenerateListTable(pn ?? "No project", itemList , showText: true, showPriority: true, showBuckets: true, showLinks: true);

                if (!string.IsNullOrEmpty(html))
                {
                    sb.Append(html);
                }
            }

            return sb.ToString();
        }

        private static HtmlTableCell GeneratePriorityCell(Item tagSet)
        {
            return new HtmlTableCell(tagSet.Priority.ToString(), classes: "cell-priority");
        }

        private static HtmlTableCell GenerateGTDCell(Item tagSet)
        {
            return new HtmlTableCell(tagSet.Bucket.ToString(), classes: "cell-timing");
        }

        private static HtmlTableCell GenerateFileNameCell(Item tagSet)
        {
            return new HtmlTableCell(tagSet.FullFileName, classes: "cell-file-name");
        }

        private static HtmlTableCell GenerateTextCell(Item ts, bool suppressProjectName = false)
        {
            string? diags = string.Join(", ", ts.Diagnostics);

            string? warnings = string.IsNullOrEmpty(ts.Warnings)
                ? null
                : $"<p class='tag-warning'>{ts.Warnings}</p>";

            string? project = suppressProjectName || string.IsNullOrEmpty(ts.ProjectName)
                ? null
                : $"<p class='cell-project-name'>Project: {ts.ProjectName}</p>";

            string? reminder = ts.IsCompleted
                ? null
                : GenerateDateString(ts.ReminderDateTime, "Reminder");

            string? due = ts.IsCompleted
                ? null
                : GenerateDateString(ts.DueDateTime, "Due");

            string? text = string.IsNullOrEmpty(ts.AssociatedText)
                ? ts.IsContext
                    ? "[Context item]"
                    : null
                : $"<p class='cell-text'>{ts.AssociatedText}</p>";

            return new HtmlTableCell(
                $"{text}{reminder}{due}{project}{warnings}",
                title: diags);
        }

        private static string? GenerateDateString(DateTime? dateTime, string? prefix)
        {
            string? dateStr = null;
            
            if (prefix is not null)
            {
                prefix += " ";
            }

            if (dateTime is not null)
            {
                DateTime dt = (DateTime)dateTime;

                string dtStr = FormatReminderDate(dt);

                if (dt.Date == DateTime.Now.Date)
                {
                    dateStr = $"<p class='reminder-date-today'>{prefix}Today</p>";
                }
                else if (dt > DateTime.Now.AddDays(3))
                {
                    dateStr = $"<p class='reminder-date-future'>{prefix}{dtStr}</p>";
                }
                else if (dt.Date > DateTime.Now.Date)
                {
                    dateStr = $"<p class='reminder-date-soon'>{prefix}{dtStr}</p>";
                }
                else
                {
                    dateStr = $"<p class='reminder-date-past'>{prefix}{dtStr}</p>";
                }
            }

            return dateStr;
        }

        private static string FormatReminderDate(DateTime dt)
        {
            string? dtStr = null;

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

        private static HtmlTableCell GenerateLinksCell(Item tagSet)
        {
            return new HtmlTableCell(
                text: "View",
                linkUrl: tagSet.FullFileName, styles: "text-align:center", title: $"{tagSet.FullFileName} line {tagSet.LineNumber} original {tagSet.RawTagSet}",
                classes: "cell-links");
        }

        private static string GenerateEventsTable(List<LogEntry> eventList)
        {
            StringBuilder sb = new(1000);

            sb.Append("<div class='table-title'>Event log</div><table class='event-log-table'><tr><th>Time</th><th>Event</th></tr>");

            foreach (var ev in eventList)
            {
                sb.Append($"<tr><td>{ev.CreatedUtc.ToString(Constants.DATE_FORMAT_TIME_ONLY_SECONDS_FRACTION)}</td><td>{ev.Text}</td></tr>");
            }

            sb.Append("</table>");

            return sb.ToString();
        }

        private static string? GenerateListTable(
            string title,
            ItemSubSet itemSubset,
            int displayOrder = 0,
            bool showText = false,
            bool showPriority = false,
            bool showLinks = false,
            bool showBuckets = false,
            bool showFileName = false,
            enPriority? onlyPriority = null,
            enBucket? onlyBucket = null,
            bool onlyDaily = false,
            bool onlyWeekly = false,
            bool onlyMonthly = false,
            string? onlyProjectName = null,
            bool suppressProjectName = false)
        {
            if (itemSubset.Any)
            {
                var filtered = itemSubset.Filter(onlyProjectName, onlyPriority, onlyBucket, onlyDaily, onlyWeekly, onlyMonthly);

                HtmlTable table = new() { DisplayOrder = displayOrder };

                var headerCells = new List<HtmlTableCell>();

                if (showText)
                    headerCells.Add(new HtmlTableCell("Text", classes: "cell-text", isHeader: true));

                if (showPriority)
                    headerCells.Add(new HtmlTableCell("Priority", classes: "cell-priority", isHeader: true));

                if (showBuckets)
                    headerCells.Add(new HtmlTableCell("GTD", classes: "cell-timing", isHeader: true));

                if (showFileName)
                    headerCells.Add(new HtmlTableCell("File", classes: "cell-file-name", isHeader: true));

                if (showLinks)
                    headerCells.Add(new HtmlTableCell("Links", classes: "cell-links", isHeader: true));

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

                        if (showPriority)
                            bodyCells.Add(GeneratePriorityCell(item));

                        if (showBuckets)
                            bodyCells.Add(GenerateGTDCell(item));

                        if (showFileName)
                            bodyCells.Add(GenerateFileNameCell(item));

                        if (showLinks)
                            bodyCells.Add(GenerateLinksCell(item));

                        HtmlTableRow row = new()
                        {
                            IsHeader = true,
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

        private static string GenerateTabHeader(string id, string title, string titleHeader)
        {
            string titleHtml = GenerateHeader(title, titleHeader, "tab-header");

            return $"<button class=\"nav-link\" id=\"nav-{id}-tab\" data-bs-toggle=\"tab\" data-bs-target=\"#nav-{id}\" type=\"button\" role=\"tab\" aria-controls=\"nav-{id}\" aria-selected=\"false\">{titleHtml}</button>";
        }

        private static string GenerateTabContent(string id, string html)
        {
            return $"<div class=\"tab-pane fade\" id=\"nav-{id}\" role=\"tabpanel\" aria-labelledby=\"nav-{id}-tab\">{html}</div>";
        }

        private static string GenerateHeader(string title, string? titlePrefix, string headerClass)
        {
            string? prefixHtml = titlePrefix is null
                ? null
                : $"<div class='prefix'>{titlePrefix}</div>";

            return $"<div class='{headerClass}'>{prefixHtml}<div class='name'>{title}</div></div>";
        }

        private static string GenerateTab(string title, string? titlePrefix, string contentHtml)
        {
            string tabId = $"{title}{titlePrefix}".ToLower();
            string titleHtml = GenerateHeader(title, titlePrefix, "tab-header");
            string tabHtml = $"<div class=\"tab\" id=\"{tabId}\"><a href = \"#{tabId}\">{titleHtml}</a><div class=\"content\">{contentHtml}</div></div>";

            return tabHtml;
        }
    }
}
