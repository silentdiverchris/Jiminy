using Jiminy.Helpers;
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

        public Item(string? bucketName, enPriority priority, string? associatedText = null, string? projectName = null, DateTime? reminderDateTime = null)
        {
            BucketName = bucketName;
            Priority = priority;
            ReminderDateTime = reminderDateTime;
            AssociatedText = associatedText;
            ProjectName = projectName;
        }

        public string? AssociatedText { get; set; } = null;

        public enPriority Priority { get; set; } = enPriority.Low;
        public string? BucketName { get; set; } = null;
        public enRepeat Repeat { get; set; } = enRepeat.None;

        public string? ProjectName { get; set; } = null;
        public DateTime? ReminderDateTime { get; set; } = null;
        public DateTime? DueDateTime { get; set; } = null;
        public bool IsCompleted { get; set; } = false;
        public bool IsContext { get; set; } = false;

        public string? FullFileName { get; set; }
        public List<string> Diagnostics { get; set; } = new();
        public string? RawTagSet { get; set; }
        public string? Warnings { get; set; }
        public int? LineNumber { get; set; }
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;

        public TagInstanceList TagInstances { get; set; } = new();

        public string? TableDisplayClass { get; set; }

        public new string ToString()
        {
            string str = $"Pri:{Priority,-6} Bucket:{BucketName,-7}";

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

        public enDateStatus ReminderStatus => ReminderDateTime.DateStatus(out _);

        public enDateStatus GetReminderStatus(out string colour)
        {
            enDateStatus ds = ReminderDateTime.DateStatus(out colour);

            return ds;
        }

        public enDateStatus DueStatus => DueDateTime.DateStatus(out _);

        public enDateStatus GetDueStatus(out string colour)
        {
            enDateStatus ds = DueDateTime.DateStatus(out colour);

            return ds;
        }
    }
}
