using System.Text.RegularExpressions;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    public class TagDefinition
    {
        /// On settings load, this is set by matching the 
        /// tag name against the enum string value, any tags in the 
        /// settings that don't match are assumed to be custom 
        /// tags (and are set to enStandardTagType 0 - None).
        public enTagType Type { get; set; } = enTagType.Custom;

        /// <summary>
        /// A view in the output will be generated on the basis of this tag
        /// </summary>
        public bool GenerateView { get; set; } = false;

        /// <summary>
        /// Should be unique, an error will be generted on startup if duplicates are found.
        /// </summary>
        public string Name { get; set; } = "";

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

        /// <summary>
        /// On startup, SVG files will be read in and stored here, and the width, height and fill properties
        /// be set up with placeholders '{0}', '{1}' and '{2}' where width, height fill values are so they can be
        /// completed using string.Format()
        /// </summary>
        //public string? SVGString { get; private set; }

        public string Code => Name.Replace(" ", "").ToLower();
        public bool IsCustomTag => Type == enTagType.Custom;
        public bool IsStandardTag => !IsCustomTag;

        public Result ValidationResult
        {
            get
            {
                Result result = new();

                if (Name.Length < 3)
                {
                    result.AddError($"Tag name '{Name}' is too short, 3 characters minimum");
                }

                if (new Regex(@"^[a-zA-Z0-9\s,]*$").IsMatch(Name) == false)
                {
                    result.AddError($"Tag name '{Name}' is invalid, it must be alphanumeric");
                }

                return result;
            }
        }
    }

    public class TagDefinitionList
    {
        public List<TagDefinition> Tags { get; set; } = new();

        public bool Exists(string name)
        {
            return Get(name) is not null;
        }

        public TagDefinition? Get(string name, bool allowPartial = false)
        {
            var found = Tags.FirstOrDefault(_ => _.Name == name);

            if (found is null)
            {
                foreach (var td in Tags.Where(_ => _.Synonyms.Any()))
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
                found = Tags.FirstOrDefault(_ => _.Name.ToLower().StartsWith(name.ToLower()));

                //if (found is null)
                //{
                //    foreach (var td in Tags.Where(_ => _.Synonyms.Any()))
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

                foreach (var td in Tags)
                {
                    result.SubsumeResult(td.ValidationResult);
                }

                var duplicateNames = Tags.GroupBy(_ => _.Name).Where(g => g.Count() > 1).Select(y => y).ToList();

                foreach (var dn in duplicateNames)
                {
                    result.AddError($"Duplicate tag name '{dn}'");
                }

                // TODO check for duplicate synonyms

                return result;
            }
        }
    }
}
