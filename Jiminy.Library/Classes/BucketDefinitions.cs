using System.Text.RegularExpressions;

namespace Jiminy.Classes
{
    public class BucketDefinition
    {
        /// <summary>
        /// Should be unique, an error will be generted on startup if duplicates are found.
        /// </summary>
        public string Name { get; set; } = "";

        public short DisplayOrder { get; set; } = 0;

        public List<string> Synonyms { get; set; } = new();

        /// <summary>
        /// Human description, will be shown as a popup when user floats over the icon, and possibly elsewhere
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// This is used to display icons in the output, some have a '{value}' portion, which will
        /// be replaced by the text values in enPriority for a priority tag, enDateStatus in the case of 
        /// reminders and due dates, ebBucket for bucket items etc.
        /// On startup, this will be filled out to the full path of the file by combining it with
        /// the MediaDirectoryPath setting.
        /// </summary>
        public string? IconFileName { get; set; } = null;

        public Result ValidationResult
        {
            get
            {
                Result result = new();

                if (Name.Length < 2)
                {
                    result.AddError($"Bucket name '{Name}' is too short, 2 characters minimum");
                }

                if (new Regex(@"^[a-zA-Z0-9\s,]*$").IsMatch(Name) == false)
                {
                    result.AddError($"Bucket name '{Name}' is invalid, it must be alphanumeric");
                }

                return result;
            }
        }
    }

    public class BucketDefinitionList
    {
        public List<BucketDefinition> Buckets { get; set; } = new();

        public bool Exists(string name)
        {
            return Get(name) is not null;
        }

        public BucketDefinition? Get(string name, bool allowPartial = false)
        {
            var found = Buckets.FirstOrDefault(_ => _.Name == name);

            if (found is null)
            {
                foreach (var td in Buckets.Where(_ => _.Synonyms.Any()))
                {
                    if (td.Synonyms.Any(_ => _ == name))
                    {
                        found = td;
                        break;
                    }
                }
            }

            if (found is null && allowPartial)
            {
                found = Buckets.FirstOrDefault(_ => _.Name.ToLower().StartsWith(name.ToLower()));

                //if (found is null)
                //{
                //    foreach (var td in Buckets.Where(_ => _.Synonyms.Any()))
                //    {
                //        if (td.Synonyms.Any(_ => _ == name))
                //        {
                //            found = td;
                //            break;
                //        }
                //    }
                //}
            }

            return found;
        }

        public Result ValidationResult
        {
            get
            {
                Result result = new();

                foreach (var bu in Buckets)
                {
                    result.SubsumeResult(bu.ValidationResult);
                }

                var duplicateNames = Buckets.GroupBy(_ => _.Name).Where(g => g.Count() > 1).Select(y => y).ToList();

                foreach (var dn in duplicateNames)
                {
                    result.AddError($"Duplicate bucket name '{dn}'");
                }

                // TODO check for duplicate synonyms

                return result;
            }
        }
    }
}
