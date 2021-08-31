﻿using Jiminy.Helpers;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    internal class Item
    {
        private readonly DateTime _imminentThreshold = DateTime.UtcNow.AddDays(2);

        public Item()
        {

        }

        public Item(string? priorityName = null, string? associatedText = null)
        {
            PriorityName = priorityName;
            AssociatedText = associatedText;
        }

        public Item(string? bucketName = null, string? priorityName = null, int? priorityNumber = null, string? associatedText = null, string? projectName = null, DateTime? reminderDateTime = null, DateTime? dueDateTime = null, string? repeatName = null)
        {
            BucketName = bucketName;
            PriorityName = priorityName;
            PriorityNumber = priorityNumber;
            DueDateTime = dueDateTime;
            ReminderDateTime = reminderDateTime;
            AssociatedText = associatedText;
            ProjectName = projectName;
            RepeatName = repeatName;
        }

        public DateTime? ReminderDateTime {get; private set; } = null;
        public DateTime? DueDateTime { get; private set; } = null;

        public string? AssociatedText { get; set; } = null;

        public string? PriorityName { get; set; } = null;
        public int? PriorityNumber { get; set; } = null;
        public string? BucketName { get; set; } = null;
        public string? RepeatName { get; set; } = null;

        public string? ProjectName { get; set; } = null;
        public bool IsCompleted { get; set; } = false;
        public bool IsContext { get; set; } = false;

        public string? FullFileName { get; set; }
        public List<string> Diagnostics { get; set; } = new();
        public string? RawTagSet { get; set; }
        public List<string> Warnings { get; set; } = new();
        public int? LineNumber { get; set; }
        public DateTime? CreatedUtc { get; set; } = DateTime.UtcNow;

        public TagInstanceList TagInstances { get; set; } = new();

        public string? TableDisplayClass { get; set; }

        public new string ToString()
        {
            string str = $"Repeat:{RepeatName}, Pri:{PriorityName}, Bucket:{BucketName}";

            if (ProjectName.NotEmpty())
            {
                str += ", Project:" + ProjectName;
            }

            if (ReminderDateTime is not null)
            {
                str += $", Reminder:{((DateTime)ReminderDateTime).ToString(Constants.DATE_FORMAT_DATE_TIME_LONG_SECONDS)}";
            }

            if (DueDateTime is not null)
            {
                str += $", Due:{((DateTime)DueDateTime).ToString(Constants.DATE_FORMAT_DATE_TIME_LONG_SECONDS)}";
            }

            str += ", Text:" + AssociatedText;

            str += ", File:" + FullFileName;

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

        internal bool IsOverdue { get; private set; }

        internal bool IsImminent { get; private set; }

        internal bool IsFuture { get; private set; } 

        internal void SetReminderDateTime(DateTime? dt)
        {
            ReminderDateTime = dt;
            SetDatedProperties();
        }

        internal void SetDueDateTime(DateTime? dt)
        {
            DueDateTime = dt;
            SetDatedProperties();
        }

        private void SetDatedProperties()
        {
            IsOverdue = !IsCompleted && (ReminderDateTime < DateTime.Now.Date || DueDateTime < DateTime.Now.Date);
            IsImminent = !IsOverdue && !IsCompleted && (ReminderDateTime < _imminentThreshold || DueDateTime < _imminentThreshold);
            IsFuture = !IsCompleted && (ReminderDateTime >= _imminentThreshold || DueDateTime >= _imminentThreshold);
        }
    }
}
