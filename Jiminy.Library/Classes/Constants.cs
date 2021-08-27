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

        //internal const string HTML_PLACEHOLDER_PROJECT_LIST = "[ProjectList]";
        internal const string HTML_PLACEHOLDER_PRIORITY_1_LIST = "[Priority1]";
        internal const string HTML_PLACEHOLDER_PRIORITY_2_LIST = "[Priority2]";
        internal const string HTML_PLACEHOLDER_PRIORITY_3_LIST = "[Priority3]";
        internal const string HTML_PLACEHOLDER_BUCKET_IN_LIST = "[InBucket]";
        internal const string HTML_PLACEHOLDER_BUCKET_NEXT_LIST = "[NextBucket]";
        internal const string HTML_PLACEHOLDER_BUCKET_WAIT_LIST = "[WaitBucket]";
        internal const string HTML_PLACEHOLDER_BUCKET_SOMEDAY_LIST = "[SomedayBucket]";
        internal const string HTML_PLACEHOLDER_TICKLER_LIST = "[TicklerList]";
        internal const string HTML_PLACEHOLDER_COMPLETED_LIST = "[CompletedList]";
        internal const string HTML_PLACEHOLDER_DAILY_LIST = "[DailyList]";
        internal const string HTML_PLACEHOLDER_WEEKLY_LIST = "[WeeklyList]";
        internal const string HTML_PLACEHOLDER_MONTHLY_LIST = "[MonthlyList]";
        internal const string HTML_PLACEHOLDER_PROJECT_LIST_GROUP = "[ProjectGroups]";
        internal const string HTML_PLACEHOLDER_FOUND_IN_FILES_LIST = "[FoundInFilesList]";
        internal const string HTML_PLACEHOLDER_EVENT_LOG = "[EventLog]";
        internal const string HTML_PLACEHOLDER_ALL_ITEMS = "[AllItems]";

        internal const string HTML_PLACEHOLDER_PROJECTS_TABS = "[ProjectsTabs]";
        internal const string HTML_PLACEHOLDER_ALL_PROJECTS_TAB = "[ProjectsTab]";
        internal const string HTML_PLACEHOLDER_ALL_BUCKETS_TAB = "[BucketsTab]";
        internal const string HTML_PLACEHOLDER_ALL_PRIORITIES_TAB = "[PrioritiesTab]";


        //internal const string HTML_PLACEHOLDER_PROJECT_TAB_HEADERS = "[ProjectTabHeaders]";
        //internal const string HTML_PLACEHOLDER_PROJECT_TAB_CONTENTS = "[ProjectTabContents]";
    }
}
