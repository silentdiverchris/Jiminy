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
            string txt = showUrl ? url : GetLinkIconHtml();
            return $"<a href='{url}' title='{url}' target='_blank'>{txt}</a>";
        }

        private static string GetLinkIconHtml(int width = 24, int height = 24)
        {
            return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" fill=\"currentColor\" class=\"bi bi-link\" viewBox=\"0 0 16 16\"><path d =\"M6.354 5.5H4a3 3 0 0 0 0 6h3a3 3 0 0 0 2.83-4H9c-.086 0-.17.01-.25.031A2 2 0 0 1 7 10.5H4a2 2 0 1 1 0-4h1.535c.218-.376.495-.714.82-1z\"/><path d =\"M9 5.5a3 3 0 0 0-2.83 4h1.098A2 2 0 0 1 9 6.5h3a2 2 0 1 1 0 4h-1.535a4.02 4.02 0 0 1-.82 1H12a3 3 0 1 0 0-6H9z\"/></svg>";
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
