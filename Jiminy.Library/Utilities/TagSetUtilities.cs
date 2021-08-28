using Jiminy.Classes;
using Jiminy.Helpers;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Utilities
{
    internal static class TagSetUtilities
    {
        internal static Result ExtractTagSet(string line, MarkdownSettings settings, out Item? tagSet)
        {
            Result result = new("ExtractTagSet");

            tagSet = null;

            foreach (var delSet in settings.TagDelimiterSets)
            {
                int idxStart = -1;
                int idxEnd = -1;
                string tagString = "";

                if (line.StartsWith(delSet.StartDelim))
                {
                    idxStart = line.IndexOf(delSet.StartDelim);
                    idxEnd = line.IndexOf(delSet.EndDelim, idxStart + 1);
                }
                else if (line.EndsWith(delSet.EndDelim))
                {
                    idxEnd = line.Length - 1;
                    idxStart = line[..^1].LastIndexOf(delSet.StartDelim);
                }

                if (idxStart >= 0 && idxEnd >= 0)
                {
                    if (idxEnd > 0)
                    {
                        tagString = line.Substring(idxStart + 1, idxEnd - idxStart - 1);
                    }
                    else
                    {
                        tagString = line.Substring(idxStart + 1);
                    }

                    string[] tagParts = tagString.Trim().Split(delSet.SepDelim);

                    result.SubsumeResult(DecodeTagString(tagParts, delSet.QualifierDelim, out tagSet));

                    if (idxEnd > 0)
                    {
                        string lineWithoutTags = line.Replace(line[idxStart..(idxEnd + 1)], "").Trim();
                        string lineWithoutMarkdown = StripMarkdown(lineWithoutTags);

                        tagSet.AssociatedText = lineWithoutMarkdown;
                    }
                    else
                    {
                        string lineWithoutTags = line.Replace(line[idxStart..], "").Trim();
                        string lineWithoutMarkdown = StripMarkdown(lineWithoutTags);

                        tagSet.AssociatedText = lineWithoutMarkdown;
                    }

                    tagSet.RawTagSet = tagString;

                    break;
                }
            }

            return result;
        }

        private static string StripMarkdown(string text)
        {
            while (text.StartsWith("#"))
            {
                text = text.Substring(1);
            }

            // Could do more :)

            return text.Trim();
        }

        private static Result DecodeTagString(string[] tagParts, string qualifierDelimiter, out Item ts)
        {
            Result result = new("DecodeTagString");

            ts = new();

            if (tagParts.Length > 0)
            {
                foreach (var tagPart in tagParts)
                {
                    if (tagPart.Length == 0)
                    {
                        result.AddWarning($"Part of the tag is empty");
                        break;
                    }

                    if (tagPart.IsDigits())
                    {
                        // has to be a priority

                        ts.Diagnostics.Add($"Setting priority {tagPart}");

                        if (tagPart.Length == 1)
                        {
                            if (short.TryParse(tagPart, out short pri))
                            {
                                if (pri <= (short)enPriority.Low)
                                {
                                    ts.Priority = (enPriority)pri;
                                }
                                else
                                {
                                    result.AddWarning($"Priority must be 1, 2 or 3");
                                }
                            }
                            else
                            {
                                result.AddWarning($"Failed to parse priority");
                            }
                        }
                        else if (tagPart.Length > 1)
                        {
                            result.AddWarning($"'{tagPart}' has multiple digits, priority must be 1, 2 or 3");
                        }
                    }
                    else
                    {
                        // Character

                        ts.Diagnostics.Add($"decoding {tagPart}");

                        char firstChar = tagPart[0];

                        if ("inwcdkmsx".Contains(firstChar))
                        {
                            if (tagPart.Length > 1)
                            {
                                result.AddWarning($"TagPart {firstChar} should just be a single character");
                                ts.Diagnostics.Add("Ignored characters after the first");
                            }
                        }

                        switch (firstChar)
                        {
                            case 'i':
                                {
                                    ts.Bucket = enBucket.In;
                                    break;
                                }
                            case 'n':
                                {
                                    ts.Bucket = enBucket.Next;
                                    break;
                                }
                            case 'w':
                                {
                                    ts.Bucket = enBucket.Waiting;
                                    break;
                                }
                            case 's':
                                {
                                    ts.Bucket = enBucket.Someday;
                                    break;
                                }
                            case 'p':
                                {
                                    // Project
                                    string? qual = GetTagQualifier(tagPart, qualifierDelimiter);

                                    if (qual is not null)
                                    {
                                        ts.Diagnostics.Add($"Setting project {qual}");
                                        ts.ProjectName = qual;
                                    }
                                    else
                                    {
                                        result.AddWarning($"TagPart '{tagPart}' no project name supplied");
                                    }

                                    break;
                                }
                            case 'c':
                                {
                                    // Sets the context for subsequent tags
                                    ts.Diagnostics.Add($"Marking this as context: {ts.ToString()}");
                                    ts.IsContext = true;
                                    break;
                                }
                            case 'r':
                                {
                                    // Reminder

                                    string? qual = GetTagQualifier(tagPart, qualifierDelimiter);

                                    if (!string.IsNullOrEmpty(qual))
                                    {
                                        DateTime? extractedDateTime = ExtractDateTime(qual);

                                        if (extractedDateTime is not null)
                                        {
                                            ts.Diagnostics.Add($"Setting reminder {extractedDateTime}");
                                            ts.ReminderDateTime = extractedDateTime;
                                        }
                                        else
                                        {
                                            // Less standard date/time, TODO
                                            result.AddWarning($"TagPart '{tagPart}' cannot extract reminder date/time as the value cannot be parsed, assuming today");
                                            ts.Diagnostics.Add($"Setting reminder for today as date not readable");
                                            ts.ReminderDateTime = DateTime.Now;
                                        }
                                    }
                                    else
                                    {
                                        result.AddWarning($"TagPart '{tagPart}' cannot extract reminder date/time as none supplied, assuming today");
                                        ts.Diagnostics.Add($"Setting reminder for today as date not supplied");
                                        ts.ReminderDateTime = DateTime.Now;
                                    }

                                    break;
                                }
                            case 'u':
                                {
                                    // Due date

                                    string? qual = GetTagQualifier(tagPart, qualifierDelimiter);

                                    if (!string.IsNullOrEmpty(qual))
                                    {
                                        DateTime? extractedDateTime = ExtractDateTime(qual);

                                        if (extractedDateTime is not null)
                                        {
                                            ts.Diagnostics.Add($"Setting due date {extractedDateTime}");
                                            ts.DueDateTime = extractedDateTime;
                                        }
                                        else
                                        {
                                            // Less standard date/time, TODO
                                            result.AddWarning($"TagPart '{tagPart}' cannot extract due date/time");
                                        }
                                    }
                                    else
                                    {
                                        ts.Diagnostics.Add($"Setting due today (default)");
                                        ts.DueDateTime = DateTime.Now;
                                    }

                                    break;
                                }
                            case 'd':
                                {
                                    ts.Diagnostics.Add($"Setting daily");
                                    ts.IsDaily = true;
                                    break;
                                }
                            case 'k':
                                {
                                    ts.Diagnostics.Add($"Setting weekly");
                                    ts.IsWeekly = true;
                                    break;
                                }
                            case 'm':
                                {
                                    ts.Diagnostics.Add($"Setting monthly");
                                    ts.IsMonthly = true;
                                    break;
                                }
                            case 'x':
                                {
                                    ts.Diagnostics.Add($"Setting completed");
                                    ts.IsCompleted = true;
                                    break;
                                }
                            default:
                                {
                                    ts.Diagnostics.Add($"Unsupported first character");
                                    result.AddWarning($"TagPart '{tagPart}' unsupported first character");
                                    break;
                                }
                        }
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
                return tagPart.Substring(tagPart.IndexOf(sep) + 1).Trim();
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
    }
}
