﻿using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Jiminy.Classes
{
    /// <summary>
    /// A directory to be monitored by a file watcher
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

        [JsonIgnore]
        public bool Exists { get; set; } = true;

        public bool IsActive { get; set; } = true;
        public string Path { get; set; }
    }
}
