using Jiminy.Helpers;
using System.Linq;
using System.Text.Json.Serialization;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    internal class Item
    {
        private readonly TagInstanceList _tagInstances = new();        
        private readonly DateTime _imminentThreshold = DateTime.UtcNow.AddDays(2);

        public string? AssociatedText { get; set; } = null;
        
        public bool SetsContext { get; set; } = false;
        public bool ClearsContext { get; set; } = false;

        public int? LineNumber { get; set; }        
        public string? FullText { get; set; }

        [JsonIgnore]
        public string? RawTagSet { get; set; }

        public string? FullFileName { get; set; }
        
        public List<string> Warnings { get; private set; } = new();
        public List<string> Diagnostics { get; private set; } = new();

        public DateTime? CreatedUtc { get; private set; } = DateTime.UtcNow;
                
        public bool IsCompleted => _tagInstances.Tags.Any(_ => _.Type == enTagType.Completed);

        [JsonIgnore]
        public TagInstance? Due => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Due);

        public DateTime? DueDateTime => Due?.DateTimeValue;

        [JsonIgnore]
        public TagInstance? Reminder => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Reminder);

        public DateTime? ReminderDateTime => Reminder?.DateTimeValue;

        [JsonIgnore]
        public TagInstance? Repeat => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Repeating);

        public string? RepeatName => Repeat?.RepeatName;

        [JsonIgnore]
        public TagInstance? Bucket => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Bucket);

        public string? BucketName => Bucket?.BucketName;

        [JsonIgnore]
        public TagInstance? Project => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Project);

        public string? ProjectName => Project?.ProjectName;
        
        [JsonIgnore]
        public TagInstance? Priority => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Priority);
        
        public string? PriorityName => Priority?.PriorityName;
        public int? PriorityNumber => Priority?.PriorityNumber;

        internal IReadOnlyList<TagInstance> TagInstances => _tagInstances.Tags.AsReadOnly();

        internal TagInstance? GetTagInstance(enTagType type)
        {
            return _tagInstances.Tags.SingleOrDefault(_ => _.Type == type);
        }

        internal bool HasTagInstance(enTagType type)
        {
            return _tagInstances.Tags.Any(_ => _.Type == type);
        }

        internal void AddTagInstance(TagInstance? ti)
        {
            if (ti is not null)
            {
                _tagInstances.Add(ti);

                switch (ti.Type)
                {
                    case enTagType.Due:
                    case enTagType.Reminder:
                    case enTagType.Repeating:
                    case enTagType.Completed:
                        {
                            SetDatedProperties();
                            break;
                        }
                }
            }
        }

        internal new string ToString()
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

        private void SetDatedProperties()
        {
            IsOverdue = !IsCompleted && (ReminderDateTime < DateTime.Now.Date || DueDateTime < DateTime.Now.Date);
            IsImminent = !IsCompleted && !IsOverdue && (ReminderDateTime < _imminentThreshold || DueDateTime < _imminentThreshold);
            IsFuture = !IsCompleted && (ReminderDateTime >= _imminentThreshold || DueDateTime >= _imminentThreshold);

            Diagnostics.Add($"Completed:{IsCompleted.YesNo()} Imminent:{IsImminent.YesNo()} Overdue:{IsOverdue.YesNo()}");
        }
    }
}
