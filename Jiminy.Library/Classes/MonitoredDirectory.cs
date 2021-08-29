namespace Jiminy.Classes
{
    /// <summary>
    /// A directory to be monitored, this is non-recursive, only the root files are monitored
    /// </summary>
    public class MonitoredDirectory
    {
        public MonitoredDirectory(string path, bool recursive, bool isActive = true)
        {
            if (path is not null && Directory.Exists(path))
            {
                Path = path;
                Recursive = recursive;
                Exists = true;
                IsActive = isActive;
            }
            else
            {
                throw new Exception($"MonitoredDirectory constructor was given null or invalid path '{path}'");
            }
        }

        /// <summary>
        /// Not suported yet
        /// </summary>
        public bool Recursive { get; set; } = false;

        public string IncludeFileSpecification { get; set; } = "*.md";

        public bool Exists { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string Path { get; set; }
    }
}
