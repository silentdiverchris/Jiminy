using System.Text.RegularExpressions;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    public class TagDefinition : BaseDefinition
    {
        public enTagType Type { get; set; } = enTagType.Custom;

        /// <summary>
        /// A view in the output will be generated on the basis of this tag
        /// </summary>
        public bool GenerateView { get; set; } = false;

        public List<string> Synonyms { get; set; } = new();

        public string Code => Name.Replace(" ", "").ToLower();
        public bool IsCustomTag => Type == enTagType.Custom;
        public bool IsStandardTag => !IsCustomTag;

        //public new Result ValidationResult
        //{
        //    get
        //    {
        //        Result result = new();

        //        if (Name.Length < 3)
        //        {
        //            result.AddError($"Tag name '{Name}' is too short, 3 characters minimum");
        //        }

        //        if (new Regex(@"^[a-zA-Z0-9\s,]*$").IsMatch(Name) == false)
        //        {
        //            result.AddError($"Tag name '{Name}' is invalid, it must be alphanumeric");
        //        }

        //        return result;
        //    }
        //}
    }

    public class TagDefinitionList
    {
        public List<TagDefinition> Items { get; set; } = new();

        public bool Exists(string name)
        {
            return Get(name) is not null;
        }

        public TagDefinition? Get(string name, bool allowPartial = false)
        {
            var found = Items.FirstOrDefault(_ => _.Name == name);

            if (found is null)
            {
                foreach (var td in Items.Where(_ => _.Synonyms.Any()))
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
                found = Items.FirstOrDefault(_ => _.Name.ToLower().StartsWith(name.ToLower()));

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

        public Result Validate()
        {
            Result result = new();

            foreach (var td in Items)
            {
                result.SubsumeResult(td.Validate("Tag"));
            }

            var duplicateNames = Items.GroupBy(_ => _.Name).Where(g => g.Count() > 1).Select(y => y).ToList();

            foreach (var dn in duplicateNames)
            {
                result.AddError($"Duplicate tag name '{dn}'");
            }

            // TODO check for duplicate synonyms

            return result;
        }
    }
}
