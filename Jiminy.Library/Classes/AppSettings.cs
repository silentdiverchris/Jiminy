namespace Jiminy.Classes
{
    public class AppSettings
    {
        public int LatencySeconds { get; set; } = 10;

        public LogSettings LogSettings { get; set; } = new LogSettings();

        public List<string> IgnoreFileSpecifications { get; set; } = new();

        /// <summary>
        /// The directories that will be monitored by the system
        /// </summary>
        public List<MonitoredDirectory> MonitoredDirectories { get; set; } = new();

        /// <summary>
        /// The settings that determine how the tags are dug out and interpreted
        /// </summary>
        public MarkdownSettings MarkdownSettings { get; set; } = new MarkdownSettings();

        /// <summary>
        /// Setting relating to building the HTML output file
        /// </summary>
        public HtmlSettings HtmlSettings { get; set; } = new HtmlSettings();
    }
}
