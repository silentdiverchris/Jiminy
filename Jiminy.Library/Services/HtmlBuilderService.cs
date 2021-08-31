using Jiminy.Classes;
using Jiminy.Helpers;
using System.Drawing;
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
                        _tagService.ProcessEmbeddedUrls(item, "<span class='card-text-link-placeholder'>[link]</span>", false);
                    }

                    StringBuilder sbTabHeaders = new(1000);
                    StringBuilder sbTabContent = new(1000);

                    if (itemRegistry.DatedItems.Any)
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
                    sbContent.Append("<div class='container'><div class='tab-wrap'>");
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

            sb.Append(GenerateTabBodyHeader("Reminders"));

            sb.Append("<div class='card-grid-container'>");

            sb.Append(GenerateItemCardSet(
                title: "Overdue",
                items: itemRegistry.OverdueItems,
                showBuckets: true,
                showPriority: true,
                showText: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Overdue", subHeader: true)));

            sb.Append(GenerateItemCardSet(
                title: "Imminent",
                items: itemRegistry.ImminentItems,
                showBuckets: true,
                showPriority: true,
                showText: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Imminent", subHeader: true)));

            sb.Append(GenerateItemCardSet(
                title: "Future",
                items: itemRegistry.FutureItems,
                showBuckets: true,
                showPriority: true,
                showText: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Future", subHeader: true)));

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
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

        private static string GenerateTabBodyHtml(string bodyHtml)
        {
            string html = $"<div class=\"tab__content\">{bodyHtml}</div>";

            return html;
        }

        private string GenerateOtherTabCollectionContent(ItemRegistry itemRegistry, List<LogEntry> logEntries)
        {
            StringBuilder sb = new(2000);

            StringBuilder sbTabHeaders = new(2000);
            StringBuilder sbTabContent = new(2000);

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Completed Items", true));
            sbTabContent.Append(GenerateCompletedItemTabContent(itemRegistry));

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Open Items"));
            sbTabContent.Append(GenerateOpenItemTabContent(itemRegistry));

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Event Log"));
            sbTabContent.Append(GenerateTabBodyHtml(GenerateEventsTable(logEntries)));

            sb.Append("<div class='container'><div class='tab-wrap'>");
            sb.Append(sbTabHeaders);
            sb.Append(sbTabContent);
            sb.Append("</div></div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateProjectTabCollectionContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(8000);

            StringBuilder sbTabHeaders = new(2000);
            StringBuilder sbTabContent = new(2000);

            bool activeTab = true;

            var projectTagDef = _appSettings.TagSettings.Defintions.Get("Project");

            string? iconHtml = projectTagDef is not null
                ? _tagService.GenerateIconItem(fileName: projectTagDef.IconFileName, overrideColour: projectTagDef.Colour)
                : null;

            foreach (var project in itemRegistry.ProjectRegistry.Projects.OrderBy(_ => _.Name))
            {
                sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_PROJECT, project.Name, activeTab));
                sbTabContent.Append(GenerateProjectTabContent(itemRegistry, project, iconHtml: iconHtml));

                activeTab = false;
            }

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_PROJECT, "All Projects", false));
            sbTabContent.Append(GenerateProjectTabContent(itemRegistry, project: null, iconHtml: iconHtml));

            sb.Append("<div class='container'><div class='tab-wrap'>");
            sb.Append(sbTabHeaders);
            sb.Append(sbTabContent);
            sb.Append("</div></div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateCompletedItemTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            sb.Append(GenerateItemCardSet(
                "Completed Items",
                itemRegistry.CompletedItems,
                showText: true,
                showPriority: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Completed Items")));

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateOpenItemTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            sb.Append(GenerateItemCardSet(
                "Open Items",
                itemRegistry.OpenItems,
                showText: true,
                showPriority: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Open Items")));

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateProjectTabContent(ItemRegistry itemRegistry, Project? project, string? iconHtml = null)
        {
            string projectName = project?.Name ?? "All projects";

            var projectItems = itemRegistry.ProjectItems.Filter(onlyProjectName: project?.Name);

            StringBuilder sb = new(2000);

            sb.Append(GenerateTabBodyHeader(projectName, iconHtml: iconHtml));

            sb.Append("<div class='card-grid-container'>");

            foreach (var bucket in _appSettings.BucketSettings.Defintions.Buckets.OrderBy(_ => _.DisplayOrder))
            {
                sb.Append(GenerateItemCardSet(
                    title: bucket.Name,
                    items: projectItems,
                    onlyBucketName: bucket.Name,
                    showText: true,
                    showPriority: true,
                    showLinks: true,
                    suppressProjectDisplay: project is not null,
                    subHeaderHtml: GenerateTabBodyHeader(bucket.Name, subHeader: true)));
            }

            var noBucketItems = new ItemSubSet(projectItems.Items.Where(_ => _.BucketName is null));

            sb.Append(GenerateItemCardSet("No Bucket", noBucketItems, showText: true, showPriority: true, showLinks: true, suppressProjectDisplay: project is not null));

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateBucketsTabContent(ItemRegistry itemRegistry, string? title, bool openItems)
        {
            StringBuilder sb = new(2000);

            var items = openItems ? itemRegistry.OpenItems : itemRegistry.CompletedItems;

            sb.Append(GenerateTabBodyHeader(title ?? ""));

            if (items.Any)
            {
                sb.Append("<div class='card-grid-container'>");

                foreach (var bucket in _appSettings.BucketSettings.Defintions.Buckets.OrderBy(_ => _.DisplayOrder))
                {
                    sb.Append(GenerateItemCardSet(
                        title: bucket.Name,
                        items: items,
                        onlyBucketName: bucket.Name,
                        showText: true,
                        showPriority: true,
                        showLinks: true,
                        suppressProjectDisplay: false,
                        subHeaderHtml: GenerateTabBodyHeader(bucket.Name, titleSuffix: "bucket", subHeader: true)));
                }

                sb.Append("</div>");
            }
            else
            {
                sb.Append(GenerateNoContentHtml());
            }

            return GenerateTabBodyHtml(sb.ToString());
        }

        private static string GenerateNoContentHtml()
        {
            string msg = "There are no items to view here";
            return $"<div class='nothing-to-see-here'><img style='margin-top:20px' src='https://i.imgflip.com/20pfsv.jpg' title='{msg}' alt='{msg}'></div>";
        }

        private string GeneratePrioritiesTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            //var priTagDef = _appSettings.TagSettings.Defintions.Get("Priority");

            //string? iconHtml = priTagDef is not null
            //    ? _tagService.GenerateIconItem(fileName: priTagDef.IconFileName, overrideColour: priTagDef.Colour)
            //    : null;

            sb.Append(GenerateTabBodyHeader("Priorities")); //, iconHtml: iconHtml));

            sb.Append("<div class='card-grid-container'>");

            foreach (var pri in _appSettings.PrioritySettings.Defintions.Priorities.OrderBy(_ => _.Number))
            {
                sb.Append(GenerateItemCardSet(
                    title: pri.Name,
                    items: itemRegistry.OpenItems,
                    onlyPriorityName: pri.Name,
                    showText: true,
                    showLinks: true,
                    suppressProjectDisplay: false,
                    subHeaderHtml: GenerateTabBodyHeader(pri.Name, subHeader: true, titleSuffix: "priority")));
            }

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateRepeatingTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            var repeatTagDef = _appSettings.TagSettings.Defintions.Get("Repeating");

            string? iconHtml = repeatTagDef is not null
                ? _tagService.GenerateIconItem(fileName: repeatTagDef.IconFileName, overrideColour: repeatTagDef.Colour)
                : null;

            sb.Append(GenerateTabBodyHeader("Repeating Items", iconHtml: iconHtml));

            sb.Append("<div class='card-grid-container'>");

            foreach (var rep in _appSettings.RepeatSettings.Defintions.Repeats.OrderBy(_ => _.DisplayOrder))
            {
                sb.Append(GenerateItemCardSet(
                    title: rep.Name,
                    items: itemRegistry.OpenItems,
                    onlyRepeatName: rep.Name,
                    showText: true,
                    showLinks: true,
                    suppressProjectDisplay: false,
                    subHeaderHtml: GenerateTabBodyHeader(rep.Name, subHeader: true)));
            }

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateProjectListGroup(ItemSubSet projectItems)
        {
            var projectNames = projectItems.Items
                .OrderBy(_ => _.ProjectName)
                .Select(_ => _.ProjectName).Distinct();

            StringBuilder sb = new(1000);

            foreach (var pn in projectNames)
            {
                var itemList = new ItemSubSet(projectItems.Items.Where(_ => _.ProjectName == pn).OrderBy(_ => _.BucketName).ThenBy(_ => _.PriorityNumber));
                string? html = GenerateItemCardSet(pn ?? "No project", itemList, showText: true, showPriority: true, showBuckets: true, showLinks: true);

                if (html.NotEmpty())
                {
                    sb.Append(html);
                }
            }

            return sb.ToString();
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
                string colour = dateTime.DateColour();

                DateTime dt = (DateTime)dateTime;
                string dtStr = dt.DisplayFriendly(true);

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

        private string GenerateItemCard(
            Item item,
            bool showText = false,
            bool showPriority = false,
            bool showLinks = false,
            bool showBucket = false,
            bool showFileName = false,
            bool suppressProjectDisplay = false)
        {
            StringBuilder sb = new(2000);

            sb.Append($"<div class='card {(item.IsOverdue ? " overdue" : item.IsImminent ? " imminent" : null)}'>");

            // Texts
            sb.Append($"<div class='item-text'>{item.AssociatedText}</div>");

            // Icons
            sb.Append($"<div class='item-icon-container'>");
            foreach (var ti in item.TagInstances.Tags.OrderBy(_ => _.Definition.DisplayOrder).ThenBy(_ => _.Name))
            {
                if (suppressProjectDisplay == false || ti.Type != enTagType.Project)
                {
                    sb.Append(_tagService.GenerateIconItem(ti));
                }
            }
            sb.Append(_tagService.GenerateIconItem(fileName: Constants.ICON_FILE_NAME_MARKDOWN_FILE, linkUrl: item.FullFileName, overrideColour: "darkgrey", overrideText: $"{item.FullFileName} #{item.LineNumber}"));
            sb.Append($"</div>");

            sb.Append(item.Warnings.Join("<div class='item-warnings'>", "<div>", "</div>", "</div>", 1000));
            
            if (_appSettings.HtmlSettings.ShowDiagnostics)
            {
                sb.Append(item.Diagnostics.Join("<div class='item-diagnostics'>", "<div>", "</div>",  "</div>", 1000));
            }

            sb.Append($"</div>");

            return sb.ToString();
        }

        private string? GenerateItemCardSet(
            string title,
            ItemSubSet items,
            int displayOrder = 0,
            bool showText = false,
            bool showPriority = false,
            bool showLinks = false,
            bool showBuckets = false,
            bool showFileName = false,
            string? onlyPriorityName = null,
            string? onlyBucketName = null,
            string? onlyRepeatName = null,
            string? onlyProjectName = null,
            bool suppressProjectDisplay = false,
            string? subHeaderHtml = null)
        {
            if (items.Any)
            {
                var filtered = items.Filter(onlyProjectName, onlyPriorityName, onlyBucketName, onlyRepeatName);

                StringBuilder sb = new(2000);

                if (filtered.Any)
                {
                    sb.Append(subHeaderHtml);

                    sb.Append("<div class='card-grid'>");

                    foreach (var item in filtered.Items.OrderByDescending(_ => _.IsOverdue).ThenByDescending(_ => _.IsImminent).ThenBy(_ => _.PriorityNumber))
                    {
                        sb.Append(GenerateItemCard(item, showText, showPriority, showLinks, showBuckets, showFileName, suppressProjectDisplay));
                    }

                    sb.Append("</div>");

                    return sb.ToString();
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

        private string GenerateTabBodyHeader(string title, string? titlePrefix = null, string? titleSuffix = null, bool subHeader = false, string? iconHtml = null)
        {
            string headerClass = subHeader
                ? "tab-body-header sub-header"
                : "tab-body-header";

            string? prefixHtml = titlePrefix is null
                ? null
                : $"<div class='prefix'>{titlePrefix}</div>";

            string? suffixHtml = titleSuffix is null
                ? null
                : $"<div class='suffix'>{titleSuffix}</div>";

            iconHtml = iconHtml is null
                ? null
                : $"<div class='icon'>{iconHtml}</div>";


            return $"<div class='{headerClass}'>{iconHtml}{prefixHtml}<div class='name'>{title}</div>{suffixHtml}</div>";
        }

        private string GenerateTab(string title, string contentHtml)
        {
            string tabId = title.ToLower();
            string titleHtml = GenerateTabBodyHeader(title);
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
