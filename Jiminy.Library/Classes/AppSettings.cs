namespace Jiminy.Classes
{
    public class AppSettings
    {
        public int LatencySeconds { get; set; } = 10;

        public string MediaDirectoryPath { get; set; } = "";

        public TagSettings TagSettings { get; set; } = new TagSettings();

        public LogSettings LogSettings { get; set; } = new LogSettings();

        public List<string> IgnoreFileSpecifications { get; set; } = new();

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
        /// The projects that items can be assigned to, these will be added to
        /// if new projects are used in the source data
        /// </summary>
        public ProjectSettings ProjectSettings { get; set; } = new ProjectSettings();

        /// <summary>
        /// The periods over which items can repeat
        /// </summary>
        public RepeatSettings RepeatSettings { get; set; } = new RepeatSettings();

        /// <summary>
        /// Setting relating to building the HTML output file
        /// </summary>
        public HtmlSettings HtmlSettings { get; set; } = new HtmlSettings();

        /// <summary>
        /// Load all SVG files into here indexed on file name for fast re-use
        /// </summary>
        public Dictionary<string, string> SvgCache = new();
    }
}
