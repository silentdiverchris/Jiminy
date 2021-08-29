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
        /// Setting relating to building the HTML output file
        /// </summary>
        public HtmlSettings HtmlSettings { get; set; } = new HtmlSettings();
    }
}
