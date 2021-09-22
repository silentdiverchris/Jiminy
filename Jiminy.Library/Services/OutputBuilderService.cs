using Jiminy.Classes;
using Jiminy.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Utilities
{
    /// <summary>
    /// Responsible for generating the output files
    /// </summary>
    internal class OutputBuilderService : IDisposable
    {
        private readonly AppSettings _appSettings;
        private readonly TagService _tagService;
        private readonly string? _itemButtonsHtml;

        public OutputBuilderService(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _tagService = new TagService(appSettings);
            _itemButtonsHtml = GenerateItemButtonsHtml();
        }

        internal async Task<Result> BuildHtml(ItemRegistry itemRegistry, List<LogEntry> logEntries, OutputSpecification of)
        {
            Result result = new("BuildHtml");

            string? templateFile = of.OverrideHtmlTemplateFileName ?? _appSettings.OutputSettings.HtmlTemplateFileName;

            if (templateFile.IsExistingFileName())
            {
                string html = await File.ReadAllTextAsync(templateFile!);
                int contentStartIdx = html.IndexOf(Constants.HTML_PLACEHOLDER_CONTENT);
                int contentEndIdx = contentStartIdx + Constants.HTML_PLACEHOLDER_CONTENT.Length;

                if (contentStartIdx == -1 || contentEndIdx == -1)
                {
                    result.AddError($"Cannot find content placeholder '{Constants.HTML_PLACEHOLDER_CONTENT}' in template file '{_appSettings.OutputSettings.HtmlTemplateFileName}'");
                }

                if (result.HasNoErrorsOrWarnings)
                {
                    // TODO this is happening per output, just do it once when the item is generated
                    foreach (var item in itemRegistry.Items)
                    {
                        _tagService.ProcessEmbeddedUrls(item, "", false); // "<span class='card-text-link-placeholder'>[link]</span>", false);
                    }

                    StringBuilder sbTabHeaders = new(1000);
                    StringBuilder sbTabContent = new(1000);

                    bool activeTab = true;

                    if (itemRegistry.DatedItems.Any)
                    {
                        // Reminders tab
                        sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Reminders", activeTab));
                        sbTabContent.Append(GenerateRemindersTabContent(itemRegistry));
                        activeTab = false;
                    }

                    // Projects tab wih sub-tabs for each project
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Projects", activeTab));
                    sbTabContent.Append(GenerateProjectTabCollectionContent(itemRegistry));

                    // Tags tab wih sub-tabs for each custom tag
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Tags"));
                    sbTabContent.Append(GenerateTagTabCollectionContent(itemRegistry));

                    // All buckets tab
                    sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_MAIN, "Buckets"));
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

                    string? titlesHtml = GenerateTitlesHtml(itemRegistry, of);
                    string? warningsHtml = GenerateWarningsHtml(itemRegistry);
                    string? ticklersHtml = GenerateTicklersHtml(itemRegistry);
                    string? footerHtml = GenerateFooterHtml(itemRegistry);

                    sbContent.Append(html.AsSpan(0, contentStartIdx));
                    sbContent.Append($"<div class='container'>{titlesHtml}{warningsHtml}{ticklersHtml}<div class='tab-wrap'>");
                    sbContent.Append(sbTabHeaders);
                    sbContent.Append(sbTabContent);
                    sbContent.Append($"</div></div>{footerHtml}");
                    sbContent.Append(html.AsSpan(contentEndIdx));

                    if (of.HtmlPath.IsExistingFileName())
                    {
                        File.Delete(of.HtmlPath!);
                    }

                    await File.WriteAllTextAsync(of.HtmlPath!, sbContent.ToString());
                }
                else
                {
                    result.AddError("Failed to generate new output file");
                }
            }
            else
            {
                result.AddError($"Html template file '{_appSettings.OutputSettings.HtmlTemplateFileName}' does not exist");
            }

            return result;
        }

        private string? GenerateTitlesHtml(ItemRegistry itemRegistry, OutputSpecification of)
        {
            StringBuilder sb = new();

            sb.Append($"<div class='page-header'><div class='header-title'>{of.Title}</div>");

            if (of.ItemSelection is not null)
            {
                sb.Append($"<div class='header-subtitle'>{of.ItemSelection.GenerateTextDescription()}</div>");
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        private string? GenerateTicklersHtml(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new();

            var list = itemRegistry.DatedItems.Items.Where(_ => _.NeedsTickler);

            if (list.Any())
            {
                sb.Append("<div class='header-ticklers'>");

                foreach (var item in list.OrderBy(_ => _.MostUrgentDateStatus))
                {
                    string classes = $"item {item.MostUrgentTagInstance?.Type.ToString().ToLower()} {item.MostUrgentDateStatus.ToString().ToLower()}";
                    sb.Append($"<div class='{classes}';>{item.MostUrgentTagInstance?.Type} - {item.MostUrgentDateStatus} - {item.AssociatedText}</div>");
                }

                sb.Append("</div>");
            }

            return sb.ToString();
        }

        private string? GenerateFooterHtml(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new();

            sb.Append("<div class='footer'>");

            sb.Append($"<div>Generated {DateTime.Now.ToString(Constants.DATE_FORMAT_DATE_TIME_LONGER_SECONDS)} - {itemRegistry.Items.Count} items found in {itemRegistry.SourceFileNames.Count} files</div>");

            sb.Append("</div>");

            return sb.ToString();
        }

        private string? GenerateWarningsHtml(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new();

            var list = itemRegistry.Items.Where(_ => _.Warnings.Any());

            if (list.Any() || itemRegistry.Duplicates.Any())
            {
                sb.Append("<div class='header-warnings'>");

                foreach (var item in itemRegistry.Duplicates)
                {
                    sb.Append("<div class='item'>");
                    sb.Append($"<div>Duplicate item in {item.SourceFileName} line {item.SourceLineNumber} - {item.AssociatedText.TruncateWithEllipsis(50)}</div>");
                    sb.Append("</div>");
                }

                foreach (var item in list)
                {
                    sb.Append("<div class='item'>");
                    sb.Append($"<div>{item.SourceFileName} line {item.SourceLineNumber}{(item.OriginalTagText.IsEmpty() ? " has no inline tags" : ", inline tags '" + item.OriginalTagText)}.</div>");

                    foreach (var warn in item.Warnings)
                    {
                        sb.Append($"<div class='text'>{warn}</div>");
                    }

                    sb.Append("</div>");
                }

                sb.Append("</div>");
            }

            return sb.ToString();
        }

        internal async Task<Result> BuildOutputs(ItemRegistry itemRegistry, List<LogEntry> recentLogEntries)
        {
            Result result = new("BuildOutputs");

            itemRegistry.CheckForDuplicates();

            foreach (var outputSpec in _appSettings.OutputSettings.Outputs.Where(_ => _.IsEnabled))
            {
                ItemRegistry itemReg = itemRegistry.GenerateRegistryForOutputFile(outputSpec);

                if (itemReg.Items.Any())
                {
                    if (outputSpec.HtmlPath.NotEmpty())
                    {
                        Result buildResult = await BuildHtml(itemReg, recentLogEntries, outputSpec);

                        if (buildResult.HasNoErrors)
                        {
                            buildResult.AddSuccess($"{DateTime.Now.ToString(Constants.DATE_FORMAT_TIME_ONLY_SECONDS)} Refreshed HTML output '{outputSpec.HtmlPath}'");
                        }

                        result.SubsumeResult(buildResult);
                    }

                    if (outputSpec.JsonPath.NotEmpty())
                    {
                        Result buildResult = await BuildJson(itemReg, recentLogEntries, outputSpec.JsonPath!);

                        if (buildResult.HasNoErrors)
                        {
                            buildResult.AddSuccess($"{DateTime.Now.ToString(Constants.DATE_FORMAT_TIME_ONLY_SECONDS)} Refreshed JSON output '{outputSpec.JsonPath}'");
                        }

                        result.SubsumeResult(buildResult);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generates a JSON output file
        /// </summary>
        /// <param name="itemReg"></param>
        /// <param name="recentLogEntries"></param>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        private async Task<Result> BuildJson(ItemRegistry itemReg, List<LogEntry> recentLogEntries, string jsonPath)
        {
            Result result = new("BuildJson");

            if (jsonPath.IsExistingFileName())
            {
                File.Delete(jsonPath);
            }

            string json = JsonSerializer.Serialize(
                value: itemReg,
                options: new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                });

            await File.WriteAllTextAsync(jsonPath, json);

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
                buttonsHtml: _itemButtonsHtml,
                showBuckets: true,
                showPriority: true,
                showText: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Overdue", subHeader: true, itemCount:itemRegistry.OverdueItems.Count)));

            sb.Append(GenerateItemCardSet(
                title: "Today",
                items: itemRegistry.TodayItems,
                buttonsHtml: _itemButtonsHtml,
                showBuckets: true,
                showPriority: true,
                showText: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Today", subHeader: true, itemCount: itemRegistry.TodayItems.Count)));

            sb.Append(GenerateItemCardSet(
                title: "Soon",
                items: itemRegistry.SoonItems,
                buttonsHtml: _itemButtonsHtml,
                showBuckets: true,
                showPriority: true,
                showText: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Soon", subHeader: true, itemCount: itemRegistry.SoonItems.Count)));

            sb.Append(GenerateItemCardSet(
                title: "Future",
                items: itemRegistry.FutureItems,
                buttonsHtml: _itemButtonsHtml,
                showBuckets: true,
                showPriority: true,
                showText: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Future", subHeader: true, itemCount: itemRegistry.FutureItems.Count)));

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        /// <summary>
        /// Gneerates the html that is used to populate the tab leaf / handle
        /// </summary>
        /// <param name="tabGroupName"></param>
        /// <param name="title"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private static string GenerateTabLeafHtml(string tabGroupName, string title, bool active = false)
        {
            string tabId = $"tab-{tabGroupName}-{title}".Replace(" ", "").ToLower();
            string checkedStr = active ? "checked" : "";

            return $"<input type=\"radio\" id=\"{tabId}\" name=\"{tabGroupName}\" class=\"tab\" {checkedStr}><label for=\"{tabId}\">{title}</label>";
        }

        /// <summary>
        /// Surrounds some html with the element to turn it into tab content
        /// </summary>
        /// <param name="bodyHtml"></param>
        /// <returns></returns>
        private static string GenerateTabBodyHtml(string bodyHtml)
        {
            return $"<div class=\"tab__content\">{bodyHtml}</div>";
        }

        /// <summary>
        /// Gneerates the 'Other' top-level tab content
        /// </summary>
        /// <param name="itemRegistry"></param>
        /// <param name="logEntries"></param>
        /// <returns></returns>
        private string GenerateOtherTabCollectionContent(ItemRegistry itemRegistry, List<LogEntry> logEntries)
        {
            StringBuilder sb = new(2000);

            StringBuilder sbTabHeaders = new(2000);
            StringBuilder sbTabContent = new(2000);

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Completed Items", true));
            sbTabContent.Append(GenerateCompletedItemTabContent(itemRegistry));

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Open Items"));
            sbTabContent.Append(GenerateOpenItemTabContent(itemRegistry));

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Hidden Items"));
            sbTabContent.Append(GenerateHiddenItemTabContent());

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_OTHER, "Event Log"));
            sbTabContent.Append(GenerateTabBodyHtml(GenerateEventsTable(logEntries)));

            sb.Append("<div class='container'><div class='tab-wrap'>");
            sb.Append(sbTabHeaders);
            sb.Append(sbTabContent);
            sb.Append("</div></div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        /// <summary>
        /// Generates tab content that contains a sub-tab for each project
        /// </summary>
        /// <param name="itemRegistry"></param>
        /// <returns></returns>
        private string GenerateProjectTabCollectionContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(8000);

            StringBuilder sbTabHeaders = new(2000);
            StringBuilder sbTabContent = new(2000);

            bool activeTab = true;

            var projectTagDef = _appSettings.TagSettings.Definitions.Get("Project");

            string? defaultIconHtml = projectTagDef is not null
                ? _tagService.GenerateIconItem(fileName: projectTagDef.IconFileName, overrideColour: projectTagDef.Colour)
                : null;

            foreach (var project in _appSettings.ProjectSettings.Definitions.Items.OrderBy(_ => _.DisplayOrder).ThenBy(_ => _.Name))
            {
                sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_PROJECT, project.Name, activeTab));

                string? projectIconHtml = project.IconFileName is null
                    ? defaultIconHtml
                    : _tagService.GenerateIconItem(fileName: project.IconFileName, overrideColour: project.Colour ?? projectTagDef?.Colour);

                sbTabContent.Append(GenerateProjectTabContent(itemRegistry, project, overrideIconHtml: projectIconHtml));

                activeTab = false;
            }

            sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_PROJECT, "All Projects", false));
            sbTabContent.Append(GenerateProjectTabContent(itemRegistry, project: null, overrideIconHtml: defaultIconHtml));

            sb.Append("<div class='container'><div class='tab-wrap'>");
            sb.Append(sbTabHeaders);
            sb.Append(sbTabContent);
            sb.Append("</div></div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        /// <summary>
        /// Generates tab content that contains a sub-tab for each tag
        /// </summary>
        /// <param name="itemRegistry"></param>
        /// <returns></returns>
        private string GenerateTagTabCollectionContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(8000);

            StringBuilder sbTabHeaders = new(2000);
            StringBuilder sbTabContent = new(2000);

            bool activeTab = true;

            //string? defaultIconHtml = projectTagDef is not null
            //    ? _tagService.GenerateIconItem(fileName: projectTagDef.IconFileName, overrideColour: projectTagDef.Colour)
            //    : null;

            foreach (var tagDef in _appSettings.TagSettings.Definitions.Items.Where(_ => _.Type == enTagType.Custom && _.GenerateView).OrderBy(_ => _.DisplayOrder).ThenBy(_ => _.Name))
            {
                sbTabHeaders.Append(GenerateTabLeafHtml(Constants.TAB_GROUP_TAGS, tagDef.Name, activeTab));

                string? iconHtml = tagDef.IconFileName is null
                    ? null
                    : _tagService.GenerateIconItem(fileName: tagDef.IconFileName, overrideColour: tagDef.Colour);

                sbTabContent.Append(GenerateTagTabContent(itemRegistry, tagDef.Name, overrideIconHtml: iconHtml));

                activeTab = false;
            }

            sb.Append("<div class='container'><div class='tab-wrap'>");
            sb.Append(sbTabHeaders);
            sb.Append(sbTabContent);
            sb.Append("</div></div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        /// <summary>
        /// Generates tab content to display all completed items
        /// </summary>
        /// <param name="itemRegistry"></param>
        /// <returns></returns>
        private string GenerateCompletedItemTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            sb.Append(GenerateItemCardSet(
                "Completed Items",
                itemRegistry.CompletedItems,
                buttonsHtml: _itemButtonsHtml,
                showText: true,
                showPriority: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Completed Items", itemCount: itemRegistry.CompletedItems.Count)));

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateHiddenItemTabContent()
        {
            StringBuilder sb = new(2000);

            sb.Append(GenerateItemCardSet(
                "Hidden Items",
                new ItemSubSet(),
                buttonsHtml: _itemButtonsHtml,
                showText: true,
                showPriority: true,
                showLinks: true,
                suppressProjectDisplay: false,
                createIfNoContent: true,
                cardGridId: "hidden-items",
                subHeaderHtml: GenerateTabBodyHeader("Hidden Items")));

            return GenerateTabBodyHtml(sb.ToString());
        }

        /// <summary>
        /// Generates tab content to display all open items
        /// </summary>
        /// <param name="itemRegistry"></param>
        /// <returns></returns>
        private string GenerateOpenItemTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            sb.Append(GenerateItemCardSet(
                "Open Items",
                itemRegistry.OpenItems,
                buttonsHtml: _itemButtonsHtml,
                showText: true,
                showPriority: true,
                showLinks: true,
                suppressProjectDisplay: false,
                subHeaderHtml: GenerateTabBodyHeader("Open Items", itemCount: itemRegistry.OpenItems.Count)));

            return GenerateTabBodyHtml(sb.ToString());
        }

        /// <summary>
        /// Generates tab content to display all items for the specified project
        /// </summary>
        /// <param name="itemRegistry"></param>
        /// <param name="project"></param>
        /// <param name="overrideIconHtml"></param>
        /// <returns></returns>
        private string GenerateProjectTabContent(
            ItemRegistry itemRegistry, 
            ProjectDefinition? project, 
            string? overrideIconHtml = null)
        {
            string projectName = project?.Name ?? "All projects";

            var items = itemRegistry.ProjectItems.Filter(onlyProject: project);

            StringBuilder sb = new(2000);

            sb.Append(GenerateTabBodyHeader(projectName, iconHtml: overrideIconHtml, itemCount: items.Count));

            sb.Append("<div class='card-grid-container'>");

            foreach (var bucket in _appSettings.BucketSettings.Definitions.Items.OrderBy(_ => _.DisplayOrder))
            {
                var iconHtml = _appSettings.SvgCache.Get(bucket.IconFileName);

                sb.Append(GenerateItemCardSet(
                    title: bucket.Name,
                    items: items,
                    buttonsHtml: _itemButtonsHtml,
                    onlyBucketName: bucket.Name,
                    showText: true,
                    showPriority: true,
                    showLinks: true,
                    suppressProjectDisplay: project is not null,
                    subHeaderHtml: GenerateTabBodyHeader(bucket.Name, subHeader: true, iconHtml: iconHtml)));
            }

            var noBucketItems = new ItemSubSet(items.Items.Where(_ => _.BucketName is null));

            sb.Append(GenerateItemCardSet(
                title: "No Bucket",
                items: noBucketItems,
                buttonsHtml: _itemButtonsHtml,
                showText: true,
                showPriority: true,
                showLinks: true,
                suppressProjectDisplay: project is not null,
                subHeaderHtml: GenerateTabBodyHeader("No bucket", subHeader: true)));

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        /// <summary>
        /// Generates tab content to display all items for the specified tag
        /// </summary>
        /// <param name="itemRegistry"></param>
        /// <param name="tagDef"></param>
        /// <param name="overrideIconHtml"></param>
        /// <returns></returns>
        private string GenerateTagTabContent(
            ItemRegistry itemRegistry,
            string tagName,
            string? overrideIconHtml = null)
        {
            var items = itemRegistry.OpenItems.Filter(onlyWithTagName: tagName);

            StringBuilder sb = new(2000);

            sb.Append(GenerateTabBodyHeader(tagName, iconHtml: overrideIconHtml, itemCount: items.Count));

            sb.Append("<div class='card-grid-container'>");

            foreach (var bucket in _appSettings.BucketSettings.Definitions.Items.OrderBy(_ => _.DisplayOrder))
            {
                sb.Append(GenerateItemCardSet(
                    title: bucket.Name,
                    items: items,
                    buttonsHtml: _itemButtonsHtml,
                    onlyBucketName: bucket.Name,
                    showText: true,
                    showPriority: true,
                    showLinks: true,
                    suppressProjectDisplay: false,
                    subHeaderHtml: GenerateTabBodyHeader(bucket.Name, subHeader: true)));
            }

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateBucketsTabContent(ItemRegistry itemRegistry, string? title, bool openItems)
        {
            StringBuilder sb = new(2000);

            var items = openItems ? itemRegistry.OpenItems : itemRegistry.CompletedItems;

            sb.Append(GenerateTabBodyHeader(title ?? "", itemCount: items.Count));

            if (items.Any)
            {
                sb.Append("<div class='card-grid-container'>");

                foreach (var bucket in _appSettings.BucketSettings.Definitions.Items.OrderBy(_ => _.DisplayOrder))
                {
                    sb.Append(GenerateItemCardSet(
                        title: bucket.Name,
                        items: items,
                        buttonsHtml: _itemButtonsHtml,
                        onlyBucketName: bucket.Name,
                        showText: true,
                        showPriority: true,
                        showLinks: true,
                        suppressProjectDisplay: false,
                        subHeaderHtml: GenerateTabBodyHeader(bucket.Name, subHeader: true, itemCount: items.Count)));
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

            foreach (var pri in _appSettings.PrioritySettings.Definitions.Items.OrderBy(_ => _.Number))
            {
                sb.Append(GenerateItemCardSet(
                    title: pri.Name,
                    items: itemRegistry.OpenItems,
                    buttonsHtml: _itemButtonsHtml,
                    onlyPriorityName: pri.Name,
                    showText: true,
                    showLinks: true,
                    suppressProjectDisplay: false,
                    subHeaderHtml: GenerateTabBodyHeader(pri.Name, subHeader: true, titleSuffix: "priority", itemCount: itemRegistry.OpenItems.Count)));
            }

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateRepeatingTabContent(ItemRegistry itemRegistry)
        {
            StringBuilder sb = new(2000);

            var repeatTagDef = _appSettings.TagSettings.Definitions.Get("Repeating");

            string? iconHtml = repeatTagDef is not null
                ? _tagService.GenerateIconItem(fileName: repeatTagDef.IconFileName, overrideColour: repeatTagDef.Colour)
                : null;

            sb.Append(GenerateTabBodyHeader("Repeating Items", iconHtml: iconHtml, itemCount: itemRegistry.OpenItems.Count));

            sb.Append("<div class='card-grid-container'>");

            foreach (var rep in _appSettings.RepeatSettings.Defintions.Items.OrderBy(_ => _.DisplayOrder))
            {
                sb.Append(GenerateItemCardSet(
                    title: rep.Name,
                    items: itemRegistry.OpenItems,
                    buttonsHtml: _itemButtonsHtml,
                    onlyRepeatName: rep.Name,
                    showText: true,
                    showLinks: true,
                    suppressProjectDisplay: false,
                    subHeaderHtml: GenerateTabBodyHeader(rep.Name, subHeader: true, itemCount: itemRegistry.OpenItems.Count)));
            }

            sb.Append("</div>");

            return GenerateTabBodyHtml(sb.ToString());
        }

        private string GenerateProjectListGroup(ItemSubSet projectItems)
        {
            StringBuilder sb = new(1000);

            foreach (var project in _appSettings.ProjectSettings.Definitions.Items.OrderBy(_ => _.DisplayOrder).ThenBy(_ => _.Name))
            {
                var itemList = new ItemSubSet(projectItems.Items.Where(_ => _.ProjectName == project.Name).OrderBy(_ => _.BucketName).ThenBy(_ => _.PriorityNumber));

                string? html = GenerateItemCardSet(
                    project.Name,
                    itemList,
                    buttonsHtml: _itemButtonsHtml,
                    showText: true,
                    showPriority: true,
                    showBuckets: true,
                    showLinks: true);

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
            bool suppressProjectDisplay = false,
            string? buttonsHtml = null)
        {
            StringBuilder sb = new(2000);

            sb.Append($"<div name='item-{item.Id}' class='card {item.MostUrgentDateStatus}'>");

            // Texts
            sb.Append($"<div class='item-text'>{item.AssociatedText}</div>");

            // Icons and buttons
            sb.Append($"<div class='item-icons-buttons-container'>");

            // Icons
            sb.Append($"<div class='item-icon-container'>{GenerateTabIcons(item, suppressProjectDisplay)}</div>");

            // Buttons
            sb.Append(buttonsHtml);

            // Icons and buttons
            sb.Append($"</div>");

            // Warnings
            if (item.Warnings.Any())
            {
                sb.Append(item.Warnings.Join("<div class='item-warnings'>", "<div>", "</div>", "</div>", 1000));
            }

            // Diagnostics
            if (_appSettings.OutputSettings.ShowDiagnostics)
            {
                List<string> diags = new();

                if (item.OriginalTagText.IsEmpty())
                {
                    diags.Add($"Raw TagSet is empty, context-only");
                }
                else
                {
                    diags.Add($"Raw TagSet '{item.OriginalTagText}'");
                }

                diags.AddRange(item.Diagnostics);

                if (_appSettings.OutputSettings.VerboseDiagnostics)
                {
                    diags.AddRange(item.TagInstances.Select(_ => _.ToString(_appSettings.OutputSettings.VerboseDiagnostics)));
                }

                sb.Append(diags.Join("<div class='item-diagnostics'>", "<div>", "</div>", "</div>", 1000));
            }

            // Card
            sb.Append($"</div>");

            return sb.ToString();
        }

        private string GenerateTabIcons(Item item, bool suppressProjectDisplay)
        {
            StringBuilder sb = new(1000);

            foreach (var ti in item.TagInstances.OrderBy(_ => _.Definition.DisplayOrder).ThenBy(_ => _.DefinitionName))
            {
                if (suppressProjectDisplay == false || ti.Type != enTagType.Project)
                {
                    sb.Append(_tagService.GenerateIconItem(ti));
                }
            }

            sb.Append(_tagService.GenerateIconItem(fileName: Constants.ICON_FILE_NAME_MARKDOWN_FILE, linkUrl: item.SourceFileName, overrideColour: "darkgrey", overrideText: $"{item.SourceFileName} #{item.SourceLineNumber}"));

            return sb.ToString();
        }

        private string GenerateItemButtonsHtml()
        {
            StringBuilder sb = new(200);

            sb.Append("<div class='item-button-container'>");
            sb.Append($"<div name='btn-promote' class='item-button disabled'>Promote</div>");
            sb.Append($"<div name='btn-demote' class='item-button disabled'>Demote</div>");
            sb.Append($"<div name='btn-hide' class='item-button disabled'>Hide</div>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private string? GenerateItemCardSet(
            string title,
            ItemSubSet items,
            string? buttonsHtml,
            int displayOrder = 0,
            bool showText = false,
            bool showPriority = false,
            bool showLinks = false,
            bool showBuckets = false,
            bool showFileName = false,
            string? onlyPriorityName = null,
            string? onlyBucketName = null,
            string? onlyRepeatName = null,
            ProjectDefinition? onlyProject = null,
            bool suppressProjectDisplay = false,
            bool createIfNoContent = false,
            string? subHeaderHtml = null,
            string? cardGridId = null)
        {
            if (items.Any || createIfNoContent)
            {
                var filtered = items.Filter(onlyProject, onlyPriorityName, onlyBucketName, onlyRepeatName);

                StringBuilder sb = new(2000);

                if (filtered.Any || createIfNoContent)
                {
                    sb.Append(subHeaderHtml);

                    sb.Append($"<div class='card-grid' id='{cardGridId}'>");

                    foreach (var item in filtered.Items.OrderBy(_ => _.MostUrgentDateStatus).ThenBy(_ => _.PriorityNumber))
                    {
                        sb.Append(GenerateItemCard(
                            item: item,
                            showText: showText,
                            showPriority: showPriority,
                            showLinks: showLinks,
                            showBucket: showBuckets,
                            showFileName: showFileName,
                            suppressProjectDisplay: suppressProjectDisplay,
                            buttonsHtml: buttonsHtml));
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

        private string GenerateTabBodyHeader(string title, string? titlePrefix = null, string? titleSuffix = null, bool subHeader = false, string? iconHtml = null, int? itemCount = null)
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

            string? countHtml = itemCount is null
                ? null
                : $"<div class='count'>{itemCount} {"item".Pluralise((int)itemCount)}</div>";

            iconHtml = iconHtml is null
                ? null
                : $"<div class='icon'>{iconHtml}</div>";


            return $"<div class='{headerClass}'>{iconHtml}{prefixHtml}<div class='name'>{title}</div>{suffixHtml}{countHtml}</div>";
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
