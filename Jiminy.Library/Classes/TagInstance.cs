using Jiminy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    /// <summary>
    /// An instance of a tag definition, one of these is created for each part 
    /// of the set of tags applied to a jiminy item, eg. one for the priority, one 
    /// for the project, one for each custom tag type etc.
    /// </summary>
    public class TagInstance
    {
        public TagInstance(
            TagDefinition definition, 
            ProjectDefinition? project = null, 
            string? priorityName = null, 
            string? bucketName = null, 
            string? repeatName = null, 
            string? url = null,
            int? priorityNumber = null, 
            DateTime? dateTime = null)
        {
            Definition = definition;
            PriorityName = priorityName;
            PriorityNumber = priorityNumber;
            BucketName = bucketName;
            DateTimeValue = dateTime;
            Project = project;
            RepeatName = repeatName;
            Url = url;
        }

        [JsonIgnore]
        public TagDefinition Definition { get; private set; }

        public string? PriorityName { get; private set; }
        public int? PriorityNumber { get; private set; }
        public string? BucketName {  get; private set; }
        public string? RepeatName { get; private set; }
        public DateTime? DateTimeValue { get; private set; }
        public ProjectDefinition? Project { get; private set; }
        public string? Url { get; private set; }

        public string DefinitionName => Definition.Name;
        public enTagType Type => Definition.Type;

        public string ToString(bool verbose)
        {
            StringBuilder sb = new();

            if (Type == enTagType.Custom)
            {
                sb.Append($"Custom Tag:{DefinitionName}");
            }
            else 
            {
                sb.Append($"Tag:{Type}");
            }

            if (BucketName.NotEmpty())
                sb.Append($" bucket {BucketName}");

            if (PriorityName.NotEmpty())
                sb.Append($" priority {PriorityName}");

            if (RepeatName.NotEmpty())
                sb.Append($" repeat {RepeatName}");

            if (Project is not null)
                sb.Append($" project {Project.Name}");

            if (DateTimeValue is not null)
                sb.Append($" {DateTimeValue.DisplayFriendly()}");

            if (verbose)
            {
                if (Definition.IconFileName is not null)
                    sb.Append($" (icon {Definition.IconFileName})");

                if (Definition.Colour is not null)
                    sb.Append($" (colour {Definition.Colour})");
            }

            return sb.ToString();
        }
    }

    public class TagInstanceList
    {
        public List<TagInstance> Tags { get; private set; } = new();

        public new string ToString()
        {
            return string.Join(", ", Tags.Select(_ => _.DefinitionName));
        }

        public bool Exists(string name)
        {
            return Tags.Any(_ => _.DefinitionName == name);
        }

        public TagInstance? Get(string name)
        {
            return Tags.FirstOrDefault(_ => _.DefinitionName == name);
        }

        public void Add(TagInstance ti, bool overwrite = false)
        {
            if (ti.Type == enTagType.Link)
            {
                // We can have as many of these as we like

                Tags.Add(ti);
            }
            else
            {
                // Only one of these is allowed

                var existing = Get(ti.DefinitionName);

                if (existing is null)
                {
                    Tags.Add(ti);
                }
                else
                {
                    if (overwrite)
                    {
                        Tags.Remove(existing);
                        Tags.Add(ti);
                    }
                    else
                    {
                        throw new Exception($"TagInstanceList.Add cannot add tag '{ti.DefinitionName}' as it already exists and overwrite is false");
                    }
                }
            }
        }
    }
}
