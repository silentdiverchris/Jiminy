namespace Jiminy.Classes
{
    internal class Constants
    {
        internal const string DATE_FORMAT_DATE_TIME_LONGER_SECONDS = "dddd d MMMM yyyy HH:mm:ss";
        internal const string DATE_FORMAT_DATE_TIME_LONG_SECONDS = "d MMM yyyy HH:mm:ss";
        internal const string DATE_FORMAT_DATE_TIME_YYYYMMDDHHMMSS = "yyyyMMddHHmmss";
        internal const string DATE_FORMAT_TIME_ONLY_SECONDS = "HH:mm:ss";
        internal const string DATE_FORMAT_TIME_ONLY_SECONDS_FRACTION = "HH:mm:ss.f";
        internal const string DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY = "ddd d MMM";
        internal const string DATE_FORMAT_DATE_TIME_REMINDER_DATE_TIME = DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY + " HH:mm";

        internal const int STREAM_BUFFER_SIZE = 81920;

        internal const int DB_TIMEOUT_SECONDS = 10;

        internal const string HTML_PLACEHOLDER_CONTENT = "[ContentPlaceholder]";

        internal const string NO_PROJECT_NAME = "No Project";
        internal const string NO_PROJECT_DESCRIPTION = "The default project when an item doesn't have one.";
        internal const string NO_PROJECT_ICON_FILE_NAME = "no-project.svg";
        internal const short NO_PROJECT_DISPLAY_ORDER = 200;

        internal const short DEFAULT_PROJECT_DISPLAY_ORDER = 100;

        internal const string TAB_GROUP_MAIN = "MainTabs";
        internal const string TAB_GROUP_PROJECT = "ProjectTabs";
        internal const string TAB_GROUP_TAGS = "TagTabs";
        internal const string TAB_GROUP_OTHER = "OtherTabs";

        internal const string ICON_FILE_NAME_VALUE = "{value}";

        internal const string ICON_FILE_NAME_MARKDOWN_FILE = "markdown.svg";
        internal const string ICON_FILE_NAME_EMBEDDED_LINK = "link.svg";

        internal const string DEFAULT_SVG_ICON_HTML = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-question\" viewBox=\"0 0 16 16\"><path d=\"M5.255 5.786a.237.237 0 0 0 .241.247h.825c.138 0 .248-.113.266-.25.09-.656.54-1.134 1.342-1.134.686 0 1.314.343 1.314 1.168 0 .635-.374.927-.965 1.371-.673.489-1.206 1.06-1.168 1.987l.003.217a.25.25 0 0 0 .25.246h.811a.25.25 0 0 0 .25-.25v-.105c0-.718.273-.927 1.01-1.486.609-.463 1.244-.977 1.244-2.056 0-1.511-1.276-2.241-2.673-2.241-1.267 0-2.655.59-2.75 2.286zm1.557 5.763c0 .533.425.927 1.01.927.609 0 1.028-.394 1.028-.927 0-.552-.42-.94-1.029-.94-.584 0-1.009.388-1.009.94z\"/></svg>";
    }
}
