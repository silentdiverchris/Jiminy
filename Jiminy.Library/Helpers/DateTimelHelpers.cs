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
                return enDateStatus.None;
            }
            else if (((DateTime)value).Date < DateTime.Now.Date)
            {
                colour = "red";
                return enDateStatus.Overdue;
            }
            else if (((DateTime)value).Date == DateTime.Now.Date)
            {
                colour = "orangered";
                return enDateStatus.Due;
            }
            else if (((DateTime)value).Date < DateTime.Now.Date.AddDays(2))
            {
                colour = "purple";
                return enDateStatus.Imminent;
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

    }
}
