using System.Text.RegularExpressions;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    public class TagDefinition : BaseDefinition
    {
        public enTagType Type { get; set; } = enTagType.Custom;

        /// <summary>
        /// A tab view in the output will be generated on the basis of this tag, this
        /// is true by default
        /// </summary>
        public bool GenerateTabView { get; set; } = true;

        public List<string> Synonyms { get; set; } = new();

        public string Code => Name.Replace(" ", "").ToLower();
        public bool IsCustomTag => Type == enTagType.Custom;
        public bool IsStandardTag => !IsCustomTag;
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
