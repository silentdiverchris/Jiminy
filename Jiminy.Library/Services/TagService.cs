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

                string[] tagParts = tagString.Trim().Split(_appSettings.TagSettings.Separator);

                result.SubsumeResult(ExtractTags(tagParts, out item));
                
                item.RawTagSet = tagString;

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

                ApplyMissingTags(item);
            }

            return result;
        }

        private void ApplyMissingTags(Item item)
        {
            // If not in a bucket, put it in the incoming one, if it exists
            if (!item.HasTagInstance(enTagType.Bucket))
            {
                var td = _appSettings.TagSettings.Defintions.Get("Bucket");
                var bucket = _appSettings.BucketSettings.Defintions.Get("Incoming");
                if (td is not null && bucket is not null)
                {
                    item.AddTagInstance(new TagInstance(td, bucketName: bucket.Name));
                }
            }

            // If no priority item, add one with the lowest defined priority
            if (!item.HasTagInstance(enTagType.Priority))
            {
                var td = _appSettings.TagSettings.Defintions.Get("Priority");
                var pri = _appSettings.PrioritySettings.Defintions.Items.OrderByDescending(_ => _.Number).FirstOrDefault();
                if (td is not null && pri is not null)
                {
                    item.AddTagInstance(new TagInstance(td, priorityName: pri.Name, priorityNumber: pri.Number));
                }
            }
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

                    item.Diagnostics.Add($"Processing tag '{tagPart}'");

                    if (tagParam.Length > 0)
                    {
                        item.Diagnostics.Add($"Code '{tagCode}', parameter '{tagParam}'");
                    }
                    else
                    {
                        item.Diagnostics.Add($"Code '{tagCode}', no parameter");
                    }

                    var td = _appSettings.TagSettings.Defintions.Get(tagCode, true);

                    if (td is not null)
                    {
                        switch (td.Type)
                        {
                            case enTagType.Custom:
                                {
                                    item.AddTagInstance(new TagInstance(td));
                                    break;
                                }
                            case enTagType.Bucket:
                                {
                                    // We extract the parameter to get the bucket name

                                    var bd = _appSettings.BucketSettings.Defintions.Get(tagParam, true);

                                    if (bd is not null)
                                    {
                                        item.AddTagInstance(new TagInstance(td, bucketName: bd.Name));
                                    }
                                    else
                                    {
                                        result.AddWarning($"Invalid bucket name '{tagParam}'");
                                    }

                                    break;
                                }
                            case enTagType.Repeating:
                                {
                                    // We expect the parameter to match a repeat name

                                    var rd = _appSettings.RepeatSettings.Defintions.Get(tagParam, true);

                                    if (rd is not null)
                                    {
                                        item.AddTagInstance(new TagInstance(td, repeatName: rd.Name));
                                    }
                                    else
                                    {
                                        result.AddWarning($"Invalid repeat name '{tagParam}'");
                                    }

                                    break;
                                }
                            case enTagType.SetContext:
                                {
                                    // Sets the context for subsequent items

                                    item.Diagnostics.Add($"Set context: {item.ToString()}");
                                    item.SetsContext = true;
                                    break;
                                }
                            case enTagType.ClearContext:
                                {
                                    // Clears any context 

                                    item.Diagnostics.Add($"Clear context");
                                    item.ClearsContext = true;
                                    break;
                                }
                            case enTagType.Due:
                            case enTagType.Reminder:
                                {
                                    // We expect the parameter to be a valid date, if not we will set the date to today

                                    DateTime? dt;

                                    if (tagParam.NotEmpty())
                                    {
                                        DateTime? extractedDateTime = ExtractDateTime(tagParam);

                                        if (extractedDateTime is not null)
                                        {
                                            dt = extractedDateTime;
                                        }
                                        else
                                        {
                                            // Less standard date/time, TODO
                                            result.AddWarning($"Cannot extract date/time as the value cannot be parsed, assuming today. try 'd/mmm' format, eg. 'r:3/sep'");
                                            item.Diagnostics.Add($"Date not readable");
                                            dt = DateTime.Now;
                                        }
                                    }
                                    else
                                    {
                                        //result.AddWarning($"TagPart '{tagPart}' cannot extract reminder date/time as none supplied, assuming today");
                                        item.Diagnostics.Add($"No date supplied");
                                        dt = DateTime.Now;
                                    }

                                    item.AddTagInstance(new TagInstance(td, dateTime: dt));

                                    break;
                                }
                            case enTagType.Priority:
                                {
                                    // We expect the parameter to match either the name or the number of a priority

                                    PriorityDefinition? pri = null;

                                    if (tagParam.IsDigits())
                                    {
                                        if (int.TryParse(tagParam, out int priNumber))
                                        {
                                            pri = _appSettings.PrioritySettings.Defintions.Get(priNumber);
                                        }
                                        else
                                        {
                                            result.AddWarning($"Failed to parse '{tagParam}' as a number");
                                        }
                                    }
                                    else
                                    {
                                        pri = _appSettings.PrioritySettings.Defintions.Get(tagParam.ToLower(), allowPartial: true);
                                    }

                                    if (pri is not null)
                                    {
                                        item.AddTagInstance(new TagInstance(td, priorityName: pri.Name, priorityNumber: pri.Number));
                                    }
                                    else
                                    {
                                        result.AddWarning($"No valid priority name or number");
                                    }

                                    break;
                                }
                            case enTagType.Project:
                                {
                                    if (tagParam.Length > 0)
                                    {
                                        item.AddTagInstance(new TagInstance(td, projectName: tagParam));
                                    }
                                    else
                                    {
                                        result.AddWarning($"No project name supplied");
                                    }

                                    break;
                                }
                            case enTagType.Completed:
                                {
                                    item.AddTagInstance(new TagInstance(td));

                                    break;
                                }
                            default:
                                {
                                    result.AddWarning($"'{tagCode}' is invalid, unrecognised tag");
                                    item.Diagnostics.Add($"No handler found for code '{tagCode}'");
                                    break;
                                }
                        }
                    }
                    else
                    {
                        result.AddWarning($"Cannot find tag from code '{tagCode}'");
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

        internal void ProcessEmbeddedUrls(Item item, string? replaceUrlWith = null, bool showUrl = false)
        {
            if (item.AssociatedText is not null)
            {
                TagDefinition? td = null;

                string text = item.AssociatedText;

                int httpIdx = text.IndexOf("http");

                if (httpIdx > -1)
                {
                    int startIdx = 0;

                    td = _appSettings.TagSettings.Defintions.Get("Link");

                    StringBuilder sb = new(text.Length + 100);

                    while (httpIdx != -1 && startIdx != -1)
                    {
                        sb.Append(text.AsSpan(startIdx, httpIdx - startIdx));

                        startIdx = text.IndexOf(" ", httpIdx);

                        if (startIdx == -1)
                        {
                            startIdx = text.IndexOf("<", httpIdx);
                        }

                        string url = startIdx == -1
                            ? text[httpIdx..]
                            : text[httpIdx..startIdx];

                        string linkHtml = MakeUrlIntoLink(url, showUrl);

                        if (td is not null)
                        {
                            item.AddTagInstance(new TagInstance(td, url: url));
                        }

                        if (replaceUrlWith is null)
                        {
                            sb.Append(linkHtml);
                        }
                        else
                        {
                            sb.Append(replaceUrlWith);
                        }

                        if (startIdx != -1)
                        { 
                            httpIdx = text.IndexOf("http", startIdx);
                        }
                    }

                    if (startIdx != -1)
                    {
                        sb.Append(text[startIdx..]);
                    }

                    item.AssociatedText = sb.ToString();
                }
            }
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
                    case enTagType.Link:
                        {
                            if (ti.Definition.IconFileName is not null)
                            {
                                iconText = $"<a class='card-link' href='{ti.Url}'>{ti.Url ?? "Missing URL"}</a>";
                                linkUrl = linkUrl ?? ti.Url;
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
                            enDateStatus status = ti.DateTimeValue.DateStatus(out colourStr);

                            iconText = ti.DateTimeValue.DisplayFriendly(showInAgo: true, prefix: "Reminder ");

                            if (ti.DateTimeValue is not null)
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
                            enDateStatus status = ti.DateTimeValue.DateStatus(out colourStr);

                            iconText = ti.DateTimeValue.DisplayFriendly(showInAgo: true, prefix: "Due ");

                            if (ti.DateTimeValue is not null)
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
                    case enTagType.Completed:
                        {
                            colourStr = ti.Definition.Colour;
                            iconText = $"Completed item";

                            if (ti.Definition.IconFileName is not null)
                            {
                                svgHtml = _appSettings.SvgCache[ti.Definition.IconFileName!];
                            }

                            break;
                        }
                    default:
                        {
                            throw new Exception($"GenerateIconItem unhandled type '{ti.Definition.Type}'");
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

            iconText = overrideText ?? iconText;
            if (iconText is not null)
            {
                sb.Append($"<div class='text'>{iconText}</div>");
            }

            sb.Append($"</div>");

            return sb.ToString();
        }
    }
}
