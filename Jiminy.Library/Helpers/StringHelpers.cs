using Jiminy.Utilities;
using System.Text;

namespace Jiminy.Helpers
{
    internal static class StringHelpers
    {
        private const string ELLIPSIS = "...";

        internal static bool NotEmpty(this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        internal static bool IsEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        internal static string? Join(this IEnumerable<string> value, string? prefixOutput = null, string? prefixItem = null, string? suffixItem = null, string? suffixOutput = null, int expectedLength = 100)
        {
            if (value.Any())
            {
                StringBuilder sb = new(expectedLength);

                sb.Append(prefixOutput);

                foreach (var str in value)
                {
                    sb.Append($"{prefixItem}{str}{suffixItem}");
                }

                sb.Append(suffixOutput);

                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

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

        internal static string? GetUrlDomain(this string? url, int maxLength)
        {
            if (string.IsNullOrEmpty(url) || url.Length <= maxLength)
                return url;

            string? text = url.Trim();

            int idxDomainStarts = text.IndexOf("//") + 2;

            int idx = idxDomainStarts;

            if (idx > -1)
            {
                int idxNextSlash = text.IndexOf("/", idx);

                while (idxNextSlash > -1 && idxNextSlash < maxLength - ELLIPSIS.Length)
                {
                    idx = idxNextSlash + 1;
                    idxNextSlash = text.IndexOf("/", idx);
                }

                text = text.Substring(idxDomainStarts, idx - idxDomainStarts) + ELLIPSIS;
            }
            else
            {
                text = text.TruncateWithEllipsis(maxLength);
            }

            return text;
        }

        internal static string? TruncateWithEllipsis(this string? text, int length, bool strictLength = false)
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
                return text.Substring(0, length - (ELLIPSIS.Length)).TrimEnd() + ELLIPSIS;
            }
            else
            {
                return text.Substring(0, length).TrimEnd() + ELLIPSIS;
            }
        }
    }
}
