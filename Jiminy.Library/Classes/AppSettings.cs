using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Jiminy.Classes
{
    public class AppSettings
    {
        public int LatencySeconds { get; set; } = 10;

        public string MediaDirectoryPath { get; set; } = "";

        public TagSettings TagSettings { get; set; } = new TagSettings();

        public LogSettings LogSettings { get; set; } = new LogSettings();

        public List<string> IgnoreFileSpecifications { get; set; } = new();

        [JsonIgnore]
        internal List<Regex> IgnoredFilesRegexList = new();

        [JsonIgnore]
        internal bool IsValid { get; set; }

        [JsonIgnore]
        internal DateTime? SettingsFileDateTime { get; set; }

        /// <summary>
        /// The directories that will be monitored by the system
        /// </summary>
        public List<MonitoredDirectory> MonitoredDirectories { get; set; } = new();

        /// <summary>
        /// The buckets that items can be put into
        /// </summary>
        public BucketSettings BucketSettings { get; set; } = new BucketSettings();

        /// <summary>
        /// The priorities that items can be assigned
        /// </summary>
        public PrioritySettings PrioritySettings { get; set; } = new PrioritySettings();

        /// <summary>
        /// The projects that items can be assigned to, projects don't need to be pre-defined
        /// as they will be created on the fly when they are found but this way you can assign 
        /// icons, colours and display orders to them.
        /// </summary>
        public ProjectSettings ProjectSettings { get; set; } = new ProjectSettings();

        /// <summary>
        /// The periods over which items can repeat
        /// </summary>
        public RepeatSettings RepeatSettings { get; set; } = new RepeatSettings();

        /// <summary>
        /// Setting relating to building the HTML and JSON output files
        /// </summary>
        public OutputSettings OutputSettings { get; set; } = new OutputSettings();

        /// <summary>
        /// All SVG file definitions are loaded into here on startup for fast 
        /// re-use, keyed on file name 
        /// </summary>
        public Dictionary<string, string> SvgCache = new();
    }
}
