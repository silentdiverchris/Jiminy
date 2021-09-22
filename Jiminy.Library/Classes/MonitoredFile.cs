using Jiminy.Helpers;
using System;
using System.IO;

namespace Jiminy.Classes
{
    public class MonitoredFile
    {
        public MonitoredFile(string fullName)
        {
            if (fullName.IsExistingFileName())
            {
                FullName = fullName;
                Exists = true;
                LastScanUtc = null;
            }
            else
            {
                throw new Exception($"MonitoredFile constructor wa given null or invalid fullName '{fullName}'");
            }
        }

        public bool Exists { get; set; } = true;
        public string? FullName { get; set; } = null;
        public DateTime? LastScanUtc { get; set; } = null;
    }
}
