using Jiminy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    internal class Item
    {
        private readonly TagInstanceList _tagInstances = new();
        private readonly List<enDateStatus> _ticklerDateStatusList = new() { enDateStatus.Today, enDateStatus.Overdue };

        public string? AssociatedText { get; set; } = null;

        public bool SetsContext { get; set; } = false;
        public bool ClearsContext { get; set; } = false;

        public int? SourceLineNumber { get; set; }
        //public string? FullText { get; set; }

        [JsonIgnore]
        public int? Hash { get; private set; }

        [JsonIgnore]
        public string? Id { get; set; }

        [JsonIgnore]
        public string? OriginalTagText { get; set; }

        [JsonIgnore]
        public string? CurrentTagSet => this.ToString();

        [JsonIgnore]
        public bool IncludeSubsequentLines { get; set; } = false;

        public string? SourceFileName { get; set; }

        public List<string> Warnings { get; private set; } = new();
        public List<string> Diagnostics { get; private set; } = new();

        public DateTime? CreatedUtc { get; private set; } = DateTime.UtcNow;

        public bool IsCompleted => _tagInstances.Tags.Any(_ => _.Type == enTagType.Completed);

        [JsonIgnore]
        public TagInstance? DueTag => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Due);

        public DateTime? DueDateTime => DueTag?.DateTimeValue;

        [JsonIgnore]
        public TagInstance? ReminderTag => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Reminder);

        public DateTime? ReminderDateTime => ReminderTag?.DateTimeValue;

        [JsonIgnore]
        public TagInstance? RepeatTag => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Repeating);

        public string? RepeatName => RepeatTag?.RepeatName;

        [JsonIgnore]
        public TagInstance? BucketTag => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Bucket);

        public string? BucketName => BucketTag?.BucketName;

        [JsonIgnore]
        public TagInstance? ProjectTag => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Project);

        public string? ProjectName => ProjectTag?.Project?.Name;

        [JsonIgnore]
        public TagInstance? Priority => _tagInstances.Tags.SingleOrDefault(_ => _.Type == enTagType.Priority);

        public string? PriorityName => Priority?.PriorityName;
        public int? PriorityNumber => Priority?.PriorityNumber;

        internal IReadOnlyList<TagInstance> TagInstances => _tagInstances.Tags.AsReadOnly();

        internal void GenerateHash()
        {
            Hash = this.ToString(includeSourceDetails: false).GetHashCode();
        }

        internal TagInstance? GetTagInstance(enTagType type)
        {
            return _tagInstances.Tags.SingleOrDefault(_ => _.Type == type);
        }

        internal bool HasTagInstance(enTagType type)
        {
            return _tagInstances.Tags.Any(_ => _.Type == type);
        }

        internal void RemoveTagInstance(string definitionName)
        {
            _tagInstances.Tags.RemoveAll(_ => _.DefinitionName == definitionName);
        }

        internal void AddTagInstance(TagInstance? ti, bool fromContext = false)
        {
            if (ti is null)
            {
                return;
            }

            // These shouldn't have got this far but just in case
            if (ti.Definition.Type == enTagType.ClearContext || ti.Definition.Type == enTagType.ClearContext)
            {
                throw new Exception($"AddTagInstance given tag type '{ti.Definition.Type}'");
            }

            if (ti is not null)
            {
                _tagInstances.Add(ti, overwrite: true);

                string diagText = $"Added {(fromContext ? "context " : "")}{(ti.Type == enTagType.Custom ? "custom " : "")}tag '{ti.DefinitionName}'";

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

                switch (ti.Type)
                {
                    case enTagType.Due:
                    case enTagType.Reminder:
                        {
                            diagText += $", '{ti.DateTimeValue.DisplayFriendly()}'";
                            break;
                        }
                    case enTagType.Repeating:
                        {
                            diagText += $", '{ti.RepeatName}'";
                            break;
                        }
                    case enTagType.Completed:
                        {
                            break;
                        }
                    case enTagType.Bucket:
                        {
                            diagText += $", '{ti.BucketName}'";
                            break;
                        }
                    case enTagType.Link:
                        {
                            diagText += $", '{ti.Url.TruncateWithEllipsis(30)}'";
                            break;
                        }
                    case enTagType.Project:
                        {
                            diagText += $", '{ti.Project?.Name}'";
                            break;
                        }
                    case enTagType.Priority:
                        {
                            diagText += $", '{ti.PriorityName}'";
                            break;
                        }
                    case enTagType.Custom:
                        {
                            // Nothing to add
                            break;
                        }
                    default:
                        {
                            diagText = $"AddTagInstance found unsupported tag '{ti.DefinitionName}'";
                            break;
                        }
                }

                Diagnostics.Add(diagText);
            }
        }

        internal string ToString(bool includeSourceDetails = true)
        {
            List<string> str = new();
            
            if (RepeatName.NotEmpty())
            {
                str.Add($"Repeat: {RepeatName}");
            }

            if (PriorityName.NotEmpty())
            {
                str.Add($"Priority: {PriorityName}");
            }
            
            if (BucketName.NotEmpty())
            {
                str.Add($"Bucket: {BucketName}");
            }

            if (ProjectName.NotEmpty())
            {
                str.Add($"Project: {ProjectName}");
            }

            if (ReminderDateTime is not null)
            {
                str.Add($"Reminder:{((DateTime)ReminderDateTime).ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY)}");
            }

            if (DueDateTime is not null)
            {
                str.Add($"Due:{((DateTime)DueDateTime).ToString(Constants.DATE_FORMAT_DATE_TIME_REMINDER_DATE_ONLY)}");
            }

            foreach (var ti in TagInstances.Where(_ => _.Type == enTagType.Custom))
            {
                str.Add(ti.DefinitionName);
            }

            if (AssociatedText.NotEmpty())
            {
                str.Add("Text:" + AssociatedText);
            }

            if (includeSourceDetails)
            {
                if (SourceFileName.NotEmpty())
                {
                    str.Add($"File: {SourceFileName} line {SourceLineNumber}");
                }
            }

            return string.Join(", ", str);
        }

        private enDateStatus GetReminderStatus(out string colour)
        {
            enDateStatus ds = ReminderDateTime.DateStatus(out colour);

            return ds;
        }

        private enDateStatus GetDueStatus(out string colour)
        {
            enDateStatus ds = DueDateTime.DateStatus(out colour);

            return ds;
        }

        public bool NeedsTickler => _ticklerDateStatusList.Contains(DueStatus) || _ticklerDateStatusList.Contains(ReminderStatus);

        // These are refreshed when any date-related property is set
        internal TagInstance? MostUrgentTagInstance { get; private set; } = null;
        internal enDateStatus MostUrgentDateStatus { get; private set; }
        internal enDateStatus DueStatus { get; private set; }
        internal enDateStatus ReminderStatus { get; private set; }

        private void SetDatedProperties()
        {
            if (IsCompleted || (ReminderDateTime is null && DueDateTime is null))
            {
                DueStatus = ReminderStatus = MostUrgentDateStatus = enDateStatus.NoDate;
            }
            else
            {
                DueStatus = GetDueStatus(out _);
                ReminderStatus = GetReminderStatus(out _);

                if ((int)DueStatus < (int)ReminderStatus)
                {
                    MostUrgentDateStatus = DueStatus;
                    MostUrgentTagInstance = TagInstances.First(_ => _.Type == enTagType.Due);
                }
                else
                {
                    MostUrgentDateStatus = ReminderStatus;
                    MostUrgentTagInstance = TagInstances.First(_ => _.Type == enTagType.Reminder);
                }
            }

            Diagnostics.Add($"Completed: {IsCompleted.YesNo()} date status: {MostUrgentDateStatus}");
        }
    }
}
