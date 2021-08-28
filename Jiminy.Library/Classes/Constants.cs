namespace Jiminy.Classes
{
    internal class Constants
    {
        internal const string DATE_FORMAT_DATE_TIME_LONG_SECONDS = "d MMM yyyy HH:mm:ss";
        internal const string DATE_FORMAT_DATE_TIME_YYYYMMDDHHMMSS = "yyyyMMddHHmmss";
        internal const string DATE_FORMAT_TIME_ONLY_SECONDS = "HH:mm:ss";
        internal const string DATE_FORMAT_TIME_ONLY_SECONDS_FRACTION = "HH:mm:ss.f";
        internal const string DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY = "dddd d MMMM";
        internal const string DATE_FORMAT_DATE_TIME_REMINDER_DATE_TIME = DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY + " HH:mm";

        internal const int STREAM_BUFFER_SIZE = 81920;

        internal const int DB_TIMEOUT_SECONDS = 10;

        internal const string HTML_PLACEHOLDER_CONTENT = "[ContentPlaceholder]";

        internal const string TAB_GROUP_MAIN = "MainTabs";
        internal const string TAB_GROUP_PROJECT = "ProjectTabs";
        internal const string TAB_GROUP_OTHER = "OtherTabs";

        //internal const string HTML_PLACEHOLDER_PROJECTS_TABS = "[ProjectsTabs]";
        //internal const string HTML_PLACEHOLDER_ALL_PROJECTS_TAB = "[ProjectsTab]";
        //internal const string HTML_PLACEHOLDER_ALL_BUCKETS_TAB = "[BucketsTab]";
        //internal const string HTML_PLACEHOLDER_ALL_PRIORITIES_TAB = "[PrioritiesTab]";
    }
}
