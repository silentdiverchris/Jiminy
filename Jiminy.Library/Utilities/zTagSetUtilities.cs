//using Jiminy.Classes;
//using Jiminy.Helpers;
//using static Jiminy.Classes.Enumerations;

//namespace Jiminy.Utilities
//{
//    internal static class TagSetUtilities
//    {
//        internal static Result ExtractTagSet(string line, MarkdownSettings settings, out Item? tagSet)
//        {
//            Result result = new("ExtractTagSet");

//            tagSet = null;

//            foreach (var delSet in settings.TagDelimiterSets)
//            {
//                int idxStart = -1;
//                int idxEnd = -1;
//                string tagString = "";

//                if (line.StartsWith(delSet.StartDelim))
//                {
//                    idxStart = line.IndexOf(delSet.StartDelim);
//                    idxEnd = line.IndexOf(delSet.EndDelim, idxStart + 1);
//                }
//                else if (line.EndsWith(delSet.EndDelim))
//                {
//                    idxEnd = line.Length - 1;
//                    idxStart = line[..^1].LastIndexOf(delSet.StartDelim);
//                }

//                if (idxStart >= 0 && idxEnd >= 0)
//                {
//                    if (idxEnd > 0)
//                    {
//                        tagString = line.Substring(idxStart + 1, idxEnd - idxStart - 1);
//                    }
//                    else
//                    {
//                        tagString = line.Substring(idxStart + 1);
//                    }

//                    string[] tagParts = tagString.Trim().Split(delSet.SepDelim);

//                    result.SubsumeResult(DecodeTagString(tagParts, delSet.QualifierDelim, out tagSet));

//                    if (idxEnd > 0)
//                    {
//                        string lineWithoutTags = line.Replace(line[idxStart..(idxEnd + 1)], "").Trim();
//                        string lineWithoutMarkdown = StripMarkdown(lineWithoutTags);

//                        tagSet.AssociatedText = lineWithoutMarkdown;
//                    }
//                    else
//                    {
//                        string lineWithoutTags = line.Replace(line[idxStart..], "").Trim();
//                        string lineWithoutMarkdown = StripMarkdown(lineWithoutTags);

//                        tagSet.AssociatedText = lineWithoutMarkdown;
//                    }

//                    tagSet.RawTagSet = tagString;

//                    break;
//                }
//            }

//            return result;
//        }

//        private static string StripMarkdown(string text)
//        {
//            while (text.StartsWith("#"))
//            {
//                text = text.Substring(1);
//            }

//            // Could do more :)

//            return text.Trim();
//        }

//        private static Result DecodeTagString(string[] tagParts, string qualifierDelimiter, out Item ts)
//        {
//            Result result = new("DecodeTagString");

//            ts = new();

//            if (tagParts.Length > 0)
//            {
//                foreach (var tagPart in tagParts)
//                {
//                    if (tagPart.Length == 0)
//                    {
//                        result.AddWarning($"Part of the tag is empty");
//                        break;
//                    }

//                    if (tagPart.IsDigits())
//                    {
//                        // has to be a priority

//                        ts.Diagnostics.Add($"Setting priority {tagPart}");

//                        if (tagPart.Length == 1)
//                        {
//                            if (short.TryParse(tagPart, out short pri))
//                            {
//                                if (pri <= (short)enPriority.Low)
//                                {
//                                    ts.Priority = (enPriority)pri;
//                                }
//                                else
//                                {
//                                    result.AddWarning($"Priority must be 1, 2 or 3");
//                                }
//                            }
//                            else
//                            {
//                                result.AddWarning($"Failed to parse priority");
//                            }
//                        }
//                        else if (tagPart.Length > 1)
//                        {
//                            result.AddWarning($"'{tagPart}' has multiple digits, priority must be 1, 2 or 3");
//                        }
//                    }
//                    else
//                    {
//                        // Character

//                        ts.Diagnostics.Add($"decoding {tagPart}");

//                        char firstChar = tagPart[0];

//                        if ("inwcdkmsx".Contains(firstChar))
//                        {
//                            if (tagPart.Length > 1)
//                            {
//                                result.AddWarning($"TagPart {firstChar} should just be a single character");
//                                ts.Diagnostics.Add("Ignored characters after the first");
//                            }
//                        }

//                        switch (firstChar)
//                        {
//                            case 'i':
//                                {
//                                    ts.Bucket = enBucket.In;
//                                    break;
//                                }
//                            case 'n':
//                                {
//                                    ts.Bucket = enBucket.Next;
//                                    break;
//                                }
//                            case 'w':
//                                {
//                                    ts.Bucket = enBucket.Waiting;
//                                    break;
//                                }
//                            case 'y':
//                                {
//                                    ts.Bucket = enBucket.Maybe;
//                                    break;
//                                }
//                            case 'p':
//                                {
//                                    // Project
//                                    string? qual = GetTagQualifier(tagPart, qualifierDelimiter);

//                                    if (qual is not null)
//                                    {
//                                        ts.Diagnostics.Add($"Setting project {qual}");
//                                        ts.ProjectName = qual;
//                                    }
//                                    else
//                                    {
//                                        result.AddWarning($"TagPart '{tagPart}' no project name supplied");
//                                    }

//                                    break;
//                                }
//                            case 'c':
//                                {
//                                    // Sets the context for subsequent tags
//                                    ts.Diagnostics.Add($"Marking this as context: {ts.ToString()}");
//                                    ts.IsContext = true;
//                                    break;
//                                }
//                            case 'r':
//                                {
//                                    // Reminder

//                                    string? qual = GetTagQualifier(tagPart, qualifierDelimiter);

//                                    if (!string.IsNullOrEmpty(qual))
//                                    {
//                                        DateTime? extractedDateTime = ExtractDateTime(qual);

//                                        if (extractedDateTime is not null)
//                                        {
//                                            ts.Diagnostics.Add($"Setting reminder {extractedDateTime}");
//                                            ts.ReminderDateTime = extractedDateTime;
//                                        }
//                                        else
//                                        {
//                                            // Less standard date/time, TODO
//                                            result.AddWarning($"TagPart '{tagPart}' cannot extract reminder date/time as the value cannot be parsed, assuming today. try 'd/mmm' format, eg. 'r:3/sep'");
//                                            ts.Diagnostics.Add($"Setting reminder for today as date not readable");
//                                            ts.ReminderDateTime = DateTime.Now;
//                                        }
//                                    }
//                                    else
//                                    {
//                                        result.AddWarning($"TagPart '{tagPart}' cannot extract reminder date/time as none supplied, assuming today");
//                                        ts.Diagnostics.Add($"Setting reminder for today as date not supplied");
//                                        ts.ReminderDateTime = DateTime.Now;
//                                    }

//                                    break;
//                                }
//                            case 'u':
//                                {
//                                    // Due date

//                                    string? qual = GetTagQualifier(tagPart, qualifierDelimiter);

//                                    if (!string.IsNullOrEmpty(qual))
//                                    {
//                                        DateTime? extractedDateTime = ExtractDateTime(qual);

//                                        if (extractedDateTime is not null)
//                                        {
//                                            ts.Diagnostics.Add($"Setting due date {extractedDateTime}");
//                                            ts.DueDateTime = extractedDateTime;
//                                        }
//                                        else
//                                        {
//                                            // Less standard date/time, TODO
//                                            result.AddWarning($"TagPart '{tagPart}' cannot extract due date/time");
//                                        }
//                                    }
//                                    else
//                                    {
//                                        ts.Diagnostics.Add($"Setting due today (default)");
//                                        ts.DueDateTime = DateTime.Now;
//                                    }

//                                    break;
//                                }
//                            case 'd':
//                                {
//                                    ts.Diagnostics.Add($"Setting daily");
//                                    ts.IsDaily = true;
//                                    break;
//                                }
//                            case 'k':
//                                {
//                                    ts.Diagnostics.Add($"Setting weekly");
//                                    ts.IsWeekly = true;
//                                    break;
//                                }
//                            case 'm':
//                                {
//                                    ts.Diagnostics.Add($"Setting monthly");
//                                    ts.IsMonthly = true;
//                                    break;
//                                }
//                            case 'x':
//                                {
//                                    ts.Diagnostics.Add($"Setting completed");
//                                    ts.IsCompleted = true;
//                                    break;
//                                }
//                            default:
//                                {
//                                    ts.Diagnostics.Add($"Unsupported first character");
//                                    result.AddWarning($"TagPart '{tagPart}' unsupported first character");
//                                    break;
//                                }
//                        }
//                    }
//                }
//            }

//            return result;
//        }

//        /// <summary>
//        /// Get the bit after the seperator, eg. 'C:xxxx' returns the 'xxxx'
//        /// </summary>
//        /// <param name="tagPart"></param>
//        /// <returns></returns>
//        private static string? GetTagQualifier(string tagPart, string sep)
//        {
//            if (tagPart.Contains(sep))
//            {
//                return tagPart.Substring(tagPart.IndexOf(sep) + 1).Trim();
//            }
//            else
//            {
//                return null;
//            }
//        }

//        private static DateTime? ExtractDateTime(string whenStr)
//        {
//            DateTime? extractedDateTime = null;

//            if (DateTime.TryParse(whenStr, out DateTime parsed))
//            {
//                extractedDateTime = parsed;
//            }
//            else
//            {
//                // Do something clever with relative dates etc. TODO
//            }

//            return extractedDateTime;
//        }

//        // https://icons.getbootstrap.com/

//        internal static string GetEmbeddedUrlIconHtml(int width = 24, int height = 24)
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"currentColor\" class=\"bi bi-link\" viewBox=\"0 0 16 16\"><path d =\"M6.354 5.5H4a3 3 0 0 0 0 6h3a3 3 0 0 0 2.83-4H9c-.086 0-.17.01-.25.031A2 2 0 0 1 7 10.5H4a2 2 0 1 1 0-4h1.535c.218-.376.495-.714.82-1z\"/><path d =\"M9 5.5a3 3 0 0 0-2.83 4h1.098A2 2 0 0 1 9 6.5h3a2 2 0 1 1 0 4h-1.535a4.02 4.02 0 0 1-.82 1H12a3 3 0 1 0 0-6H9z\"/></svg>";
//        }

//        internal static string GetReminderIconHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-alarm-fill\" viewBox=\"0 0 16 16\"><path d=\"M6 .5a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 0 1H9v1.07a7.001 7.001 0 0 1 3.274 12.474l.601.602a.5.5 0 0 1-.707.708l-.746-.746A6.97 6.97 0 0 1 8 16a6.97 6.97 0 0 1-3.422-.892l-.746.746a.5.5 0 0 1-.707-.708l.602-.602A7.001 7.001 0 0 1 7 2.07V1h-.5A.5.5 0 0 1 6 .5zm2.5 5a.5.5 0 0 0-1 0v3.362l-1.429 2.38a.5.5 0 1 0 .858.515l1.5-2.5A.5.5 0 0 0 8.5 9V5.5zM.86 5.387A2.5 2.5 0 1 1 4.387 1.86 8.035 8.035 0 0 0 .86 5.387zM11.613 1.86a2.5 2.5 0 1 1 3.527 3.527 8.035 8.035 0 0 0-3.527-3.527z\"/></svg>";
//        }

//        internal static string GetDueIconHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-calendar-x-fill\" viewBox=\"0 0 16 16\"><path d=\"M4 .5a.5.5 0 0 0-1 0V1H2a2 2 0 0 0-2 2v1h16V3a2 2 0 0 0-2-2h-1V.5a.5.5 0 0 0-1 0V1H4V.5zM16 14V5H0v9a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2zM6.854 8.146 8 9.293l1.146-1.147a.5.5 0 1 1 .708.708L8.707 10l1.147 1.146a.5.5 0 0 1-.708.708L8 10.707l-1.146 1.147a.5.5 0 0 1-.708-.708L7.293 10 6.146 8.854a.5.5 0 1 1 .708-.708z\"/></svg>";
//        }

//        internal static string GetBugIconHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-bug-fill\" viewBox=\"0 0 16 16\"><path d=\"M4.978.855a.5.5 0 1 0-.956.29l.41 1.352A4.985 4.985 0 0 0 3 6h10a4.985 4.985 0 0 0-1.432-3.503l.41-1.352a.5.5 0 1 0-.956-.29l-.291.956A4.978 4.978 0 0 0 8 1a4.979 4.979 0 0 0-2.731.811l-.29-.956z\"/><path d=\"M13 6v1H8.5v8.975A5 5 0 0 0 13 11h.5a.5.5 0 0 1 .5.5v.5a.5.5 0 1 0 1 0v-.5a1.5 1.5 0 0 0-1.5-1.5H13V9h1.5a.5.5 0 0 0 0-1H13V7h.5A1.5 1.5 0 0 0 15 5.5V5a.5.5 0 0 0-1 0v.5a.5.5 0 0 1-.5.5H13zm-5.5 9.975V7H3V6h-.5a.5.5 0 0 1-.5-.5V5a.5.5 0 0 0-1 0v.5A1.5 1.5 0 0 0 2.5 7H3v1H1.5a.5.5 0 0 0 0 1H3v1h-.5A1.5 1.5 0 0 0 1 11.5v.5a.5.5 0 1 0 1 0v-.5a.5.5 0 0 1 .5-.5H3a5 5 0 0 0 4.5 4.975z\"/></svg>";
//        }

//        internal static string GetPriorityHighIconHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-cloud-arrow-up\" viewBox=\"0 0 16 16\"><path fill-rule=\"evenodd\" d=\"M7.646 5.146a.5.5 0 0 1 .708 0l2 2a.5.5 0 0 1-.708.708L8.5 6.707V10.5a.5.5 0 0 1-1 0V6.707L6.354 7.854a.5.5 0 1 1-.708-.708l2-2z\"/><path d=\"M4.406 3.342A5.53 5.53 0 0 1 8 2c2.69 0 4.923 2 5.166 4.579C14.758 6.804 16 8.137 16 9.773 16 11.569 14.502 13 12.687 13H3.781C1.708 13 0 11.366 0 9.318c0-1.763 1.266-3.223 2.942-3.593.143-.863.698-1.723 1.464-2.383zm.653.757c-.757.653-1.153 1.44-1.153 2.056v.448l-.445.049C2.064 6.805 1 7.952 1 9.318 1 10.785 2.23 12 3.781 12h8.906C13.98 12 15 10.988 15 9.773c0-1.216-1.02-2.228-2.313-2.228h-.5v-.5C12.188 4.825 10.328 3 8 3a4.53 4.53 0 0 0-2.941 1.1z\"/></svg>";
//        }

//        internal static string GetPriorityMediumIconHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-cloud\" viewBox=\"0 0 16 16\"><path d=\"M4.406 3.342A5.53 5.53 0 0 1 8 2c2.69 0 4.923 2 5.166 4.579C14.758 6.804 16 8.137 16 9.773 16 11.569 14.502 13 12.687 13H3.781C1.708 13 0 11.366 0 9.318c0-1.763 1.266-3.223 2.942-3.593.143-.863.698-1.723 1.464-2.383zm.653.757c-.757.653-1.153 1.44-1.153 2.056v.448l-.445.049C2.064 6.805 1 7.952 1 9.318 1 10.785 2.23 12 3.781 12h8.906C13.98 12 15 10.988 15 9.773c0-1.216-1.02-2.228-2.313-2.228h-.5v-.5C12.188 4.825 10.328 3 8 3a4.53 4.53 0 0 0-2.941 1.1z\"/></svg>";
//        }

//        internal static string GetDefaultIconHtml()
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-x-lg\" viewBox=\"0 0 16 16\"><path d=\"M1.293 1.293a1 1 0 0 1 1.414 0L8 6.586l5.293-5.293a1 1 0 1 1 1.414 1.414L9.414 8l5.293 5.293a1 1 0 0 1-1.414 1.414L8 9.414l-5.293 5.293a1 1 0 0 1-1.414-1.414L6.586 8 1.293 2.707a1 1 0 0 1 0-1.414z\"/></svg>";
//        }

//        internal static string GetTagIconHtml(TagInstance ti) // Dynamic SVG updateto come... int width = 24, int height = 24, string? overrideColour = null)
//        {
//            string html = "";

//            //string? colourStr = "currentColor";
//            string? valueStr = null;
//            string fileName = ti.Definition.IconFileName;

//            switch (ti.Definition.Type)
//            {
//                case enTagType.Custom:
//                    {
//                        break;
//                    }
//                case enTagType.Priority:
//                    {
//                        valueStr = ti.Priority.ToString();
//                        break;
//                    }
//                case enTagType.Reminder:
//                    {
//                        if (ti.ReminderDateTime is not null)
//                        {
//                            valueStr = ti.ReminderDateTime.DateStatus(out _).ToString();                            
//                        }
//                        else
//                        {
//                            throw new Exception($"GetTagIconHtml found tag type reminder has no date time");
//                        }
//                        break;
//                    }
//                case enTagType.Due:
//                    {
//                        if (ti.DueDateTime is not null)
//                        {
//                            valueStr = ti.DueDateTime.DateStatus(out _).ToString();
//                        }
//                        else
//                        {
//                            throw new Exception($"GetTagIconHtml found tag type due has no date time");
//                        }
//                        break;
//                    }
//                case enTagType.Bucket:
//                    {
//                        valueStr = ti.Bucket.ToString();
//                        break;
//                    }
//                case enTagType.Project:
//                    {
//                        valueStr = ti.ProjectName;
//                        break;
//                    }
//            }

//            if (valueStr is not null)
//            {
//                fileName = fileName.Replace(Constants.ICON_FILE_NAME_VALUE, valueStr);
//            }

//            if (File.Exists(fileName))
//            {
//                html = $"<div class='tag-icon'><img src='{fileName}'></div>";
//            }
//            else
//            {
//                html = $"<div class='tag-icon'>{GetDefaultIconHtml()}</div>";
//            }


//            // If it's an SVG, load the svg definition and set the color and size
//            //if (ti.Definition.SVGString is not null)
//            //{
//            //    html = $"<div class='tag-icon'><img src='{string.Format(ti.Definition.SVGString, width, height, overrideColour ?? colourStr)}'></div>";
//            //}
//            //else
//            //{
//            //}

//            return html;
//        }

//        internal static string GetPriorityLowIconHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-cloud-arrow-down\" viewBox=\"0 0 16 16\"><path fill-rule=\"evenodd\" d=\"M7.646 10.854a.5.5 0 0 0 .708 0l2-2a.5.5 0 0 0-.708-.708L8.5 9.293V5.5a.5.5 0 0 0-1 0v3.793L6.354 8.146a.5.5 0 1 0-.708.708l2 2z\"/><path d=\"M4.406 3.342A5.53 5.53 0 0 1 8 2c2.69 0 4.923 2 5.166 4.579C14.758 6.804 16 8.137 16 9.773 16 11.569 14.502 13 12.687 13H3.781C1.708 13 0 11.366 0 9.318c0-1.763 1.266-3.223 2.942-3.593.143-.863.698-1.723 1.464-2.383zm.653.757c-.757.653-1.153 1.44-1.153 2.056v.448l-.445.049C2.064 6.805 1 7.952 1 9.318 1 10.785 2.23 12 3.781 12h8.906C13.98 12 15 10.988 15 9.773c0-1.216-1.02-2.228-2.313-2.228h-.5v-.5C12.188 4.825 10.328 3 8 3a4.53 4.53 0 0 0-2.941 1.1z\"/></svg>";
//        }

//        internal static string GetLinkIconHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-arrow-up-right-square\" viewBox=\"0 0 16 16\"><path fill-rule=\"evenodd\" d=\"M15 2a1 1 0 0 0-1-1H2a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V2zM0 2a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V2zm5.854 8.803a.5.5 0 1 1-.708-.707L9.243 6H6.475a.5.5 0 1 1 0-1h3.975a.5.5 0 0 1 .5.5v3.975a.5.5 0 1 1-1 0V6.707l-4.096 4.096z\"/></svg>";
//        }

//        internal static string GetInBucketHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-inbox\" viewBox=\"0 0 16 16\"><path d=\"M4.98 4a.5.5 0 0 0-.39.188L1.54 8H6a.5.5 0 0 1 .5.5 1.5 1.5 0 1 0 3 0A.5.5 0 0 1 10 8h4.46l-3.05-3.812A.5.5 0 0 0 11.02 4H4.98zm9.954 5H10.45a2.5 2.5 0 0 1-4.9 0H1.066l.32 2.562a.5.5 0 0 0 .497.438h12.234a.5.5 0 0 0 .496-.438L14.933 9zM3.809 3.563A1.5 1.5 0 0 1 4.981 3h6.038a1.5 1.5 0 0 1 1.172.563l3.7 4.625a.5.5 0 0 1 .105.374l-.39 3.124A1.5 1.5 0 0 1 14.117 13H1.883a1.5 1.5 0 0 1-1.489-1.314l-.39-3.124a.5.5 0 0 1 .106-.374l3.7-4.625z\"/></svg>";
//        }

//        internal static string GetNextBucketHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-exclamation-diamond-fill\" viewBox=\"0 0 16 16\"><path d=\"M9.05.435c-.58-.58-1.52-.58-2.1 0L.436 6.95c-.58.58-.58 1.519 0 2.098l6.516 6.516c.58.58 1.519.58 2.098 0l6.516-6.516c.58-.58.58-1.519 0-2.098L9.05.435zM8 4c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995A.905.905 0 0 1 8 4zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z\"/></svg>";
//        }

//        internal static string GetWaitingBucketHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-hourglass-split\" viewBox=\"0 0 16 16\"><path d=\"M2.5 15a.5.5 0 1 1 0-1h1v-1a4.5 4.5 0 0 1 2.557-4.06c.29-.139.443-.377.443-.59v-.7c0-.213-.154-.451-.443-.59A4.5 4.5 0 0 1 3.5 3V2h-1a.5.5 0 0 1 0-1h11a.5.5 0 0 1 0 1h-1v1a4.5 4.5 0 0 1-2.557 4.06c-.29.139-.443.377-.443.59v.7c0 .213.154.451.443.59A4.5 4.5 0 0 1 12.5 13v1h1a.5.5 0 0 1 0 1h-11zm2-13v1c0 .537.12 1.045.337 1.5h6.326c.216-.455.337-.963.337-1.5V2h-7zm3 6.35c0 .701-.478 1.236-1.011 1.492A3.5 3.5 0 0 0 4.5 13s.866-1.299 3-1.48V8.35zm1 0v3.17c2.134.181 3 1.48 3 1.48a3.5 3.5 0 0 0-1.989-3.158C8.978 9.586 8.5 9.052 8.5 8.351z\"/></svg>";
//        }

//        internal static string GetMaybeBucketHtml(int width = 24, int height = 24, string fillColour = "currentColor")
//        {
//            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"{fillColour}\" class=\"bi bi-piggy-bank\" viewBox=\"0 0 16 16\"><path d=\"M5 6.25a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0zm1.138-1.496A6.613 6.613 0 0 1 7.964 4.5c.666 0 1.303.097 1.893.273a.5.5 0 0 0 .286-.958A7.602 7.602 0 0 0 7.964 3.5c-.734 0-1.441.103-2.102.292a.5.5 0 1 0 .276.962z\"/><path fill-rule=\"evenodd\" d=\"M7.964 1.527c-2.977 0-5.571 1.704-6.32 4.125h-.55A1 1 0 0 0 .11 6.824l.254 1.46a1.5 1.5 0 0 0 1.478 1.243h.263c.3.513.688.978 1.145 1.382l-.729 2.477a.5.5 0 0 0 .48.641h2a.5.5 0 0 0 .471-.332l.482-1.351c.635.173 1.31.267 2.011.267.707 0 1.388-.095 2.028-.272l.543 1.372a.5.5 0 0 0 .465.316h2a.5.5 0 0 0 .478-.645l-.761-2.506C13.81 9.895 14.5 8.559 14.5 7.069c0-.145-.007-.29-.02-.431.261-.11.508-.266.705-.444.315.306.815.306.815-.417 0 .223-.5.223-.461-.026a.95.95 0 0 0 .09-.255.7.7 0 0 0-.202-.645.58.58 0 0 0-.707-.098.735.735 0 0 0-.375.562c-.024.243.082.48.32.654a2.112 2.112 0 0 1-.259.153c-.534-2.664-3.284-4.595-6.442-4.595zM2.516 6.26c.455-2.066 2.667-3.733 5.448-3.733 3.146 0 5.536 2.114 5.536 4.542 0 1.254-.624 2.41-1.67 3.248a.5.5 0 0 0-.165.535l.66 2.175h-.985l-.59-1.487a.5.5 0 0 0-.629-.288c-.661.23-1.39.359-2.157.359a6.558 6.558 0 0 1-2.157-.359.5.5 0 0 0-.635.304l-.525 1.471h-.979l.633-2.15a.5.5 0 0 0-.17-.534 4.649 4.649 0 0 1-1.284-1.541.5.5 0 0 0-.446-.275h-.56a.5.5 0 0 1-.492-.414l-.254-1.46h.933a.5.5 0 0 0 .488-.393zm12.621-.857a.565.565 0 0 1-.098.21.704.704 0 0 1-.044-.025c-.146-.09-.157-.175-.152-.223a.236.236 0 0 1 .117-.173c.049-.027.08-.021.113.012a.202.202 0 0 1 .064.199z\"/></svg>";
//        }

//        internal static string CustomiseIcon(string iconHtml, string? title = null, string? linkUrl = null, string? tipText = null)
//        {
//            iconHtml = tipText is null
//                ? iconHtml
//                : $"<a href=\"#\" class=\"tip\">{iconHtml}<span>{tipText}</span></a>";

//            if (linkUrl is null)
//            {
//                return $"<div class='cell-icon' title='{title}'>{iconHtml}</div>";
//            }
//            else
//            {
//                return $"<div class='cell-icon' title='{title}'><a href='{linkUrl}' target=@_blank'>{iconHtml}</a></div>";
//            }
//        }
//    }
//}
