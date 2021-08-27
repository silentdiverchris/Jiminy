namespace Jiminy.Helpers
{
    internal static class BoolHelpers
    {
        internal static string YesNo(this bool value, bool lowerCase = false)
        {
            string str = value ? "Yes" : "No";

            if (lowerCase)
                str = str.ToLower();

            return str;
        }
    }
}
