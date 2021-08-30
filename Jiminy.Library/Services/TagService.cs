using Jiminy.Classes;
using Jiminy.Helpers;
using System.Text;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Utilities
{
    internal class TagService
    {
        private readonly AppSettings _appSettings;

        public TagService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        internal Result ExtractTagSet(string line, out Item? item)
        {
            // TODO test with multi-character prefix and suffix

            Result result = new("ExtractTagSet");

            item = null;

            int idxStart = -1;
            int idxEnd = -1;
            string tagString = "";

            if (line.StartsWith(_appSettings.TagSettings.Prefix))
            {
                idxStart = line.IndexOf(_appSettings.TagSettings.Prefix);
                idxEnd = line.IndexOf(_appSettings.TagSettings.Suffix, idxStart + _appSettings.TagSettings.Prefix.Length);
            }
            else if (line.EndsWith(_appSettings.TagSettings.Suffix))
            {
                idxEnd = line.Length - 1;
                idxStart = line[..^1].LastIndexOf(_appSettings.TagSettings.Prefix);
            }

            if (idxStart >= 0 && idxEnd >= 0)
            {
                if (idxEnd > 0)
                {
                    tagString = line.Substring(idxStart + 1, idxEnd - idxStart - 1);
                }
                else
                {
                    tagString = line[(idxStart + 1)..];
                }

                //if (tagString.Contains("que"))
                //{
                //    var a = 1;
                //}

                string[] tagParts = tagString.Trim().Split(_appSettings.TagSettings.Seperator);

                result.SubsumeResult(ExtractTags(tagParts, out item));

                if (idxEnd > 0)
                {
                    string lineWithoutTags = line.Replace(line[idxStart..(idxEnd + 1)], "").Trim();
                    string lineWithoutMarkdown = StripMarkdown(lineWithoutTags);

                    item.AssociatedText = lineWithoutMarkdown;
                }
                else
                {
                    string lineWithoutTags = line.Replace(line[idxStart..], "").Trim();
                    string lineWithoutMarkdown = StripMarkdown(lineWithoutTags);

                    item.AssociatedText = lineWithoutMarkdown;
                }

                item.RawTagSet = tagString;

                // If no priority item, add one with the lowest defined priority
                if (!item.TagInstances.Tags.Any(_ => _.Type == enTagType.Priority))
                {
                    var td = _appSettings.TagSettings.TagDefintions.Get("Priority");
                    var pri = _appSettings.PrioritySettings.Defintions.Priorities.OrderByDescending(_ => _.Number).FirstOrDefault();
                    if (td is not null && pri is not null)
                    {
                        item.TagInstances.Add(new TagInstance(td, priorityName: pri.Name, priorityNumber: pri.Number));
                        item.PriorityName = pri.Name;
                        item.PriorityNumber = pri.Number;
                    }
                }
            }

            return result;
        }

        private static string StripMarkdown(string text)
        {
            while (text.StartsWith("#"))
            {
                text = text[1..];
            }

            // Could do more :)

            return text.Trim();
        }

        /// <summary>
        /// Take the array of little texts and decode them into items and tags
        /// </summary>
        /// <param name="tagParts"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private Result ExtractTags(string[] tagParts, out Item item)
        {
            Result result = new("DecodeTagString");

            item = new();

            if (tagParts.Length > 0)
            {
                foreach (var tagPart in tagParts)
                {
                    if (tagPart.Length == 0)
                    {
                        result.AddWarning($"Part of the tag is empty");
                        break;
                    }

                    string tagCode = tagPart;
                    string tagParam = "";

                    int delimIdx = tagPart.IndexOf(_appSettings.TagSettings.Delimiter);

                    if (delimIdx != -1)
                    {
                        tagCode = tagPart.Substring(0, delimIdx);

                        if (tagPart.Length >= delimIdx + 1)
                        {
                            tagParam = tagPart[(delimIdx + 1)..];
                        }
                        else
                        {
                            result.AddWarning($"TagPart '{tagPart}' has invalid format, cannot extract parameter");
                        }
                    }

                    item.Diagnostics.Add($"decoding {tagPart}, code '{tagCode}', parameter '{tagParam}'");

                    var td = _appSettings.TagSettings.TagDefintions.Get(tagCode, true);

                    if (td is not null)
                    {
                        switch (td.Type)
                        {
                            case enTagType.Custom:
                                {
                                    item.TagInstances.Add(new TagInstance(td));
                                    break;
                                }
                            case enTagType.Bucket:
                                {
                                    // We extract the parameter to get the bucket name

                                    var bd = _appSettings.BucketSettings.Defintions.Get(tagParam, true);

                                    if (bd is not null)
                                    {
                                        item.BucketName = bd.Name;
                                        item.TagInstances.Add(new TagInstance(td, bucketName: bd.Name));
                                    }
                                    else
                                    {
                                        result.AddWarning($"TagPart '{tagPart}' has invalid bucket name '{tagParam}'");
                                    }

                                    break;
                                }
                            case enTagType.Repeating:
                                {
                                    // We extract the parameter to match a repeat name

                                    var rd = _appSettings.RepeatSettings.Defintions.Get(tagParam, true);

                                    if (rd is not null)
                                    {
                                        item.RepeatName = rd.Name;
                                        item.TagInstances.Add(new TagInstance(td, repeatName: rd.Name));
                                    }
                                    else
                                    {
                                        result.AddWarning($"TagPart '{tagPart}' has invalid repeat name '{tagParam}'");
                                    }

                                    break;
                                }
                            case enTagType.Context:
                                {
                                    // Sets the context for subsequent items

                                    item.Diagnostics.Add($"Marking this as context: {item.ToString()}");
                                    item.IsContext = true;
                                    break;
                                }
                            case enTagType.Due:
                            case enTagType.Reminder:
                                {
                                    // We expect the parameter to be a valid date, if not we will set the date to today

                                    DateTime? dt;

                                    if (!string.IsNullOrEmpty(tagParam))
                                    {
                                        DateTime? extractedDateTime = ExtractDateTime(tagParam);

                                        if (extractedDateTime is not null)
                                        {
                                            dt = extractedDateTime;
                                        }
                                        else
                                        {
                                            // Less standard date/time, TODO
                                            result.AddWarning($"TagPart '{tagPart}' cannot extract reminder date/time as the value cannot be parsed, assuming today. try 'd/mmm' format, eg. 'r:3/sep'");
                                            item.Diagnostics.Add($"Setting reminder for today as date not readable");
                                            dt = DateTime.Now;
                                        }
                                    }
                                    else
                                    {
                                        //result.AddWarning($"TagPart '{tagPart}' cannot extract reminder date/time as none supplied, assuming today");
                                        item.Diagnostics.Add($"Setting reminder for today as date not supplied");
                                        dt = DateTime.Now;
                                    }

                                    item.Diagnostics.Add($"Setting date/time {dt}");
                                    
                                    if (td.Type == enTagType.Reminder)
                                    {
                                        item.ReminderDateTime = dt;
                                    }
                                    else if (td.Type == enTagType.Due)
                                    {
                                        item.DueDateTime = dt;
                                    }

                                    item.TagInstances.Add(new TagInstance(td, dateTime: dt));

                                    break;
                                }
                            case enTagType.Priority:
                                {
                                    // We expect the parameter to match either the name or the number of a priority

                                    item.Diagnostics.Add($"Setting priority from {tagPart}");


                                    PriorityDefinition? pri = null;

                                    if (tagParam.IsDigits())
                                    {
                                        if (int.TryParse(tagParam, out int priNumber))
                                        {
                                            pri = _appSettings.PrioritySettings.Defintions.Get(priNumber);
                                        }
                                        else
                                        {
                                            result.AddWarning($"Failed to parse priority as a number");
                                        }
                                    }
                                    else
                                    {
                                        pri = _appSettings.PrioritySettings.Defintions.Get(tagParam.ToLower(), allowPartial: true);
                                    }

                                    if (pri is not null)
                                    {
                                        item.PriorityName = pri.Name;
                                        item.PriorityNumber = pri.Number;
                                        item.TagInstances.Add(new TagInstance(td, priorityName: pri.Name, priorityNumber: pri.Number));
                                    }
                                    else
                                    {
                                        result.AddWarning($"'{tagPart}' is not a valid priority name or number");
                                    }

                                    break;
                                }
                            case enTagType.Project:
                                {
                                    if (tagParam.Length > 0)
                                    {
                                        item.Diagnostics.Add($"Setting project {tagParam}");
                                        item.ProjectName = tagParam;
                                        item.TagInstances.Add(new TagInstance(td, projectName: tagParam));
                                    }
                                    else
                                    {
                                        result.AddWarning($"TagPart '{tagPart}' no project name supplied");
                                    }

                                    break;
                                }
                            case enTagType.Completed:
                                {
                                    item.Diagnostics.Add($"Setting completed {tagParam}");
                                    item.IsCompleted = true;
                                    item.TagInstances.Add(new TagInstance(td));

                                    break;
                                }
                            default:
                                {
                                    result.AddWarning($"'{tagPart}' is invalid, unrecognised tag type");
                                    break;
                                }
                        }
                    }
                    else
                    {
                        result.AddWarning($"TagPart '{tagPart}' cannot find tag '{tagCode}'");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the bit after the seperator, eg. 'C:xxxx' returns the 'xxxx'
        /// </summary>
        /// <param name="tagPart"></param>
        /// <returns></returns>
        private static string? GetTagQualifier(string tagPart, string sep)
        {
            if (tagPart.Contains(sep))
            {
                return tagPart[(tagPart.IndexOf(sep) + 1)..].Trim();
            }
            else
            {
                return null;
            }
        }

        private static DateTime? ExtractDateTime(string whenStr)
        {
            DateTime? extractedDateTime = null;

            if (DateTime.TryParse(whenStr, out DateTime parsed))
            {
                extractedDateTime = parsed;
            }
            else
            {
                // Do something clever with relative dates etc. TODO
            }

            return extractedDateTime;
        }

        internal string? ConvertURLsToLinks(string? text, bool showUrl)
        {
            if (text is not null)
            {
                int httpIdx = text.IndexOf("http");

                if (httpIdx > -1)
                {
                    int startIdx = 0;

                    StringBuilder sb = new(text.Length + 100);

                    while (httpIdx != -1 && startIdx != -1)
                    {
                        sb.Append(text.AsSpan(startIdx, httpIdx - startIdx));

                        startIdx = text.IndexOf(" ", httpIdx);

                        if (startIdx == -1)
                        {
                            startIdx = text.IndexOf("<", httpIdx);
                        }

                        if (startIdx == -1)
                        {
                            // That was the end
                            sb.Append(MakeUrlIntoLink(text[httpIdx..], showUrl));
                        }
                        else
                        {
                            sb.Append(MakeUrlIntoLink(text[httpIdx..startIdx], showUrl));

                            httpIdx = text.IndexOf("http", startIdx);
                        }
                    }

                    if (startIdx != -1)
                    {
                        sb.Append(text[startIdx..]);
                    }

                    return sb.ToString();
                }
            }

            return text;
        }

        private string MakeUrlIntoLink(string url, bool showUrl)
        {
            string txt = showUrl ? url : GenerateIconItem(fileName: Constants.ICON_FILE_NAME_EMBEDDED_LINK);
            return $"<a href='{url}' title='{url}' target='_blank'>{txt}</a>";
        }

        internal static string GetDefaultIconHtml()
        {
            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-x-lg\" viewBox=\"0 0 16 16\"><path d=\"M1.293 1.293a1 1 0 0 1 1.414 0L8 6.586l5.293-5.293a1 1 0 1 1 1.414 1.414L9.414 8l5.293 5.293a1 1 0 0 1-1.414 1.414L8 9.414l-5.293 5.293a1 1 0 0 1-1.414-1.414L6.586 8 1.293 2.707a1 1 0 0 1 0-1.414z\"/></svg>";
        }

        internal string GenerateIconItem(TagInstance? ti = null, string? fileName = null, int width = 24, int height = 24, string? overrideColour = null, string? linkUrl = null, string? overrideText = null)
        {
            StringBuilder sb = new(1000);

            string svgHtml = "";
            string iconText = "";

            string? colourStr = overrideColour ?? "currentColor";

            if (ti is not null)
            {
                switch (ti.Definition.Type)
                {
                    case enTagType.Custom:
                        {
                            if (ti.Definition.IconFileName is not null)
                            {
                                iconText = ti.Definition.Description;
                                colourStr = ti.Definition.Colour;
                                svgHtml = _appSettings.SvgCache[ti.Definition.IconFileName!];
                            }

                            break;
                        }
                    case enTagType.Priority:
                        {
                            var pri = _appSettings.PrioritySettings.Defintions.Get((int)ti.PriorityNumber!);

                            if (pri is not null)
                            {
                                iconText = $"Priority: {pri.Name}";
                                colourStr = pri.Colour;

                                if (pri?.IconFileName is not null)
                                {
                                    svgHtml = _appSettings.SvgCache[pri.IconFileName!];
                                }
                            }

                            break;
                        }
                    case enTagType.Repeating:
                        {
                            var rep = _appSettings.RepeatSettings.Defintions.Get(ti.RepeatName!.ToLower());

                            if (rep is not null)
                            {
                                iconText = $"Repeating: {rep.Name}";
                                colourStr = rep.Colour;

                                if (rep?.IconFileName is not null)
                                {
                                    svgHtml = _appSettings.SvgCache[rep.IconFileName!];
                                }
                            }
                            else
                            {
                                throw new Exception($"GetTagIconHtml found tag type repeat has no valid period");
                            }

                            break;
                        }
                    case enTagType.Reminder:
                        {
                            enDateStatus status = ti.DateTime.DateStatus(out colourStr);

                            iconText = ti.DateTime.DisplayFriendly(showInAgo: true, prefix: "Reminder ");

                            if (ti.DateTime is not null)
                            {
                                svgHtml = _appSettings.SvgCache[ti.Definition.IconFileName!];
                            }
                            else
                            {
                                throw new Exception($"GetTagIconHtml found tag type reminder has no date time");
                            }

                            break;
                        }
                    case enTagType.Due:
                        {
                            enDateStatus status = ti.DateTime.DateStatus(out colourStr);

                            iconText = ti.DateTime.DisplayFriendly(showInAgo: true, prefix: "Due ");

                            if (ti.DateTime is not null)
                            {
                                svgHtml = _appSettings.SvgCache[ti.Definition.IconFileName!];
                            }
                            else
                            {
                                throw new Exception($"GetTagIconHtml found tag type due has no date time");
                            }

                            break;
                        }
                    case enTagType.Bucket:
                        {
                            var bucket = _appSettings.BucketSettings.Defintions.Get(ti.BucketName);

                            if (bucket is not null)
                            {
                                colourStr = bucket.Colour;
                                iconText = $"Bucket: {ti.BucketName}";

                                if (bucket?.IconFileName is not null)
                                {
                                    svgHtml = _appSettings.SvgCache[bucket.IconFileName!];
                                }
                            }

                            break;
                        }
                    case enTagType.Project:
                        {
                            colourStr = ti.Definition.Colour;
                            iconText = $"Project: {ti.ProjectName}";

                            if (ti.Definition.IconFileName is not null)
                            {
                                svgHtml = _appSettings.SvgCache[ti.Definition.IconFileName!];
                            }

                            break;
                        }
                }
            }
            else
            {
                if (fileName is not null)
                {
                    svgHtml = _appSettings.SvgCache[fileName!];
                }
            }

            if (svgHtml is null)
            {
                svgHtml = GetDefaultIconHtml();
            }

            // TODO do this properly

            svgHtml = svgHtml.Replace("width=\"16\"", $"width=\"{width}\"");
            svgHtml = svgHtml.Replace("height=\"16\"", $"width=\"{height}\"");
            svgHtml = svgHtml.Replace("fill=\"currentColor\"", $"fill=\"{colourStr}\"");
            
            sb.Append($"<div class='item-icon-item'>");

            if (linkUrl is null)
            {
                sb.Append($"<div class='icon'>{svgHtml}</div>");
            }
            else
            {
                sb.Append($"<div class='icon'><a href='{linkUrl}' title='' target='_blank'>{svgHtml}</a></div>");
            }

            sb.Append($"<div class='text'>");
            sb.Append(overrideText ?? iconText);
            sb.Append($"</div>");
            sb.Append($"</div>");

            return sb.ToString(); 
        }
    }
}
