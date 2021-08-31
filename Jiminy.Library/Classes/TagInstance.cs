﻿using Jiminy.Helpers;
using System.Text;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    public class TagInstance
    {
        public TagInstance(
            TagDefinition definition, 
            string? projectName = null, 
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
            ProjectName = projectName;
            RepeatName = repeatName;
            Url = url;
        }

        public TagDefinition Definition { get; private set; }
        public string? PriorityName { get; private set; }
        public int? PriorityNumber { get; private set; }
        public string? BucketName {  get; private set; }
        public string? RepeatName { get; private set; }
        public DateTime? DateTimeValue { get; private set; }
        public string? ProjectName { get; private set; }
        public string? Url { get; private set; }

        public string DefinitionName => Definition.Name;
        public enTagType Type => Definition.Type;

        public string ToString(bool verbose)
        {
            StringBuilder sb = new();

            sb.Append($"Tag:{Type}");

            if (BucketName.NotEmpty())
                sb.Append($" bucket {BucketName}");

            if (PriorityName.NotEmpty())
                sb.Append($" priority {PriorityName}");

            if (RepeatName.NotEmpty())
                sb.Append($" repeat {RepeatName}");

            if (ProjectName.NotEmpty())
                sb.Append($" project {ProjectName}");

            if (DateTimeValue is not null)
                sb.Append($" {DateTimeValue.DisplayFriendly()}");

            if (verbose)
            {
                if (Definition.IconFileName is not null)
                    sb.Append($" icon {Definition.IconFileName}");

                if (Definition.Colour is not null)
                    sb.Append($" colour {Definition.Colour}");
            }

            return sb.ToString();
        }
    }

    public class TagInstanceList
    {
        public List<TagInstance> Tags { get; private set; } = new();

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
