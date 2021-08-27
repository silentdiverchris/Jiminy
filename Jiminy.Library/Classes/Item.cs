using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    internal class Item
    {
        public Item()
        {

        }

        public Item(enPriority priority, string? associatedText = null)
        {
            Priority = priority;
            AssociatedText = associatedText;
        }

        public Item(enBucket bucket, enPriority priority, string? associatedText = null, string? projectName = null, DateTime? reminderDateTime = null)
        {
            Bucket = bucket;
            Priority = priority;
            ReminderDateTime = reminderDateTime;
            AssociatedText = associatedText;
            ProjectName = projectName;
        }

        public string? AssociatedText { get; set; } = null;

        public enPriority Priority { get; set; } = enPriority.Low;
        public enBucket Bucket { get; set; } = enBucket.None;

        public string? ProjectName { get; set; } = null;
        public DateTime? ReminderDateTime { get; set; } = null;
        public DateTime? DueDateTime { get; set; } = null;
        public bool IsDaily { get; set; } = false;
        public bool IsWeekly { get; set; } = false;
        public bool IsMonthly { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public bool IsContext { get; set; } = false;
        public bool IsDue { get; set; } = false;

        public string? FullFileName { get; set; }
        public List<string> Diagnostics { get; set; } = new();
        public string? RawTagSet { get; set; }
        public string? Warnings { get; set; }
        public int? LineNumber { get; set; }
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;

        public string? TableDisplayClass { get; set; }

        public new string ToString()
        {
            string str = $"Pri:{Priority,-6} Timing:{Bucket,-7}";

            if (!string.IsNullOrEmpty(ProjectName))
            {
                str += " Project:" + ProjectName;
            }

            if (ReminderDateTime is not null)
            {
                str += $" Reminder:{((DateTime)ReminderDateTime).ToString(Constants.DATE_FORMAT_DATE_TIME_LONG_SECONDS)}";
            }

            str += " Text:" + AssociatedText;

            str += " File:" + FullFileName;

            return str;
        }
    }
}
