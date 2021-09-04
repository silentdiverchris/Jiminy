using Jiminy.Classes;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Helpers
{
    internal static class DateTimeHelpers
    {
        internal static enDateStatus DateStatus(this DateTime? value, out string colour)
        {
            if (value is null)
            {
                colour = "";
                return enDateStatus.NoDate;
            }
            else if (((DateTime)value).Date < DateTime.Now.Date)
            {
                colour = "red";
                return enDateStatus.Overdue;
            }
            else if (((DateTime)value).Date == DateTime.Now.Date)
            {
                colour = "orangered";
                return enDateStatus.Today;
            }
            else if (((DateTime)value).Date < DateTime.Now.Date.AddDays(2))
            {
                colour = "purple";
                return enDateStatus.Soon;
            }
            else
            {
                colour = "green";
                return enDateStatus.Future;
            }
        }

        internal static string DateColour(this DateTime? value)
        {
            var _ = value.DateStatus(out string colour);

            return colour;
        }

        internal static string DisplayFriendly(this DateTime? value, bool showInAgo = false, string? prefix = null, string? suffix = null)
        {
            if (value is null)
            {
                return "";
            }
            else
            {
                return $"{prefix}{((DateTime)value).DisplayFriendly(showInAgo)}{suffix}";
            }
        }

        internal static string DisplayFriendly(this DateTime value, bool showInAgo = false)
        {
            string dtStr;

            if (value.Date == DateTime.Now.Date)
            {
                dtStr = "today";
            }
            else
            {
                if (value.Hour == 0 && value.Minute == 0)
                {
                    dtStr = value.ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY);
                }
                else
                {
                    dtStr = value.ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY);
                }

                if (showInAgo)
                {
                    int daysDiff = (int)(value - DateTime.Now.Date).TotalDays;

                    if (daysDiff > 0)
                    {
                        if (daysDiff == 1)
                        {
                            dtStr = "tomorrow";
                        }
                        else
                        {
                            dtStr = $"in {daysDiff} day{daysDiff.PluralSuffix()} on {value.ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY)}";
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
                            dtStr = $"{daysDiff} day{daysDiff.PluralSuffix()} ago, {value.ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY)}";
                        }
                    }
                }
            }

            return dtStr;
        }

    }
}
