﻿using Jiminy.Utilities;
using System.Text;

namespace Jiminy.Helpers
{
    internal static class StringHelpers
    {
        internal static string ConcatenateToDelimitedList(this IEnumerable<string> strings, string delimiter = ",", string quote = "'")
        {
            return quote + string.Join(delimiter, strings) + quote;
        }

        internal static bool IsDigits(this string text)
        {
            foreach (var c in text)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// A decidedly primitive implementation, not intended to work for everything, to be
        /// fixed each time a new case it doesn't handle comes up
        /// </summary>
        /// <param name="str"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        internal static string Pluralise(this string? str, int number, string? addSuffixIfNotEmpty = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            else
            {
                if (number == 1)
                {
                    return str + addSuffixIfNotEmpty;
                }
                else
                {
                    if (str.EndsWith("ry"))
                    {
                        return str[0..^1] + "ies" + addSuffixIfNotEmpty;
                    }
                    else
                    {
                        return str + "s" + addSuffixIfNotEmpty;
                    }
                }
            }
        }

        internal static string? ConvertURLsToLinks(string? text, bool showUrl)
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

        private static string MakeUrlIntoLink(string url, bool showUrl)
        {
            string txt = showUrl ? url : TagSetUtilities.GetEmbeddedUrlIconHtml();
            return $"<a href='{url}' title='{url}' target='_blank'>{txt}</a>";
        }

        internal static string TruncateWithEllipsis(this string text, int length, bool strictLength = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Trim();

            if (string.IsNullOrEmpty(text) || text.Length < length)
            {
                return text;
            }

            if (strictLength)
            {
                return text.Substring(0, length - 3).TrimEnd() + "...";
            }
            else
            {
                return text.Substring(0, length).TrimEnd() + "...";
            }
        }
    }
}
