using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    public class TagInstance
    {
        public TagInstance(TagDefinition definition, string? projectName = null, enPriority? priority = null, string? bucketName = null, enRepeat? repeat = null, DateTime? dateTime = null)
        {
            Definition = definition;
            Priority = priority;
            BucketName = bucketName;
            DateTime = dateTime;
            ProjectName = projectName;
            Repeat = repeat;
        }

        public TagDefinition Definition { get; private set; }
        public enPriority? Priority { get; private set; }
        public string? BucketName {  get; private set; }
        public enRepeat? Repeat { get; private set; }
        public DateTime? DateTime { get; private set; }
        public string? ProjectName { get; private set; }

        public string Name => Definition.Name;
        public enTagType Type => Definition.Type;
    }

    public class TagInstanceList
    {
        public List<TagInstance> Tags { get; private set; } = new();

        public bool Exists(string name)
        {
            return Tags.Any(_ => _.Name == name);
        }

        public TagInstance? Get(string name)
        {
            return Tags.FirstOrDefault(_ => _.Name == name);
        }

        public void Add(TagInstance ti, bool overwrite = false)
        {
            var existing = Get(ti.Name);

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
                    throw new Exception($"TagInstanceList.Add cannot add tag '{ti.Name}' as it already exists and ioverwrite is false");
                }
            }
        }
    }
}
