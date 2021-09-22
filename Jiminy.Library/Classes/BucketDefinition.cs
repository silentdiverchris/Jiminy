using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jiminy.Classes
{
    public class BucketDefinition : BaseDefinition
    {
        public List<string> Synonyms { get; set; } = new();
    }

    public class BucketDefinitionList
    {
        public List<BucketDefinition> Items { get; set; } = new();

        public bool Exists(string name)
        {
            return Get(name) is not null;
        }

        public BucketDefinition? Get(string? name, bool allowPartial = false)
        {
            if (name is null)
                return null;

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

        public Result Validate()
        {
            Result result = new();

            foreach (var bu in Items)
            {
                result.SubsumeResult(bu.Validate("Bucket"));
            }

            var duplicateNames = Items.GroupBy(_ => _.Name).Where(g => g.Count() > 1).Select(y => y).ToList();

            foreach (var dn in duplicateNames)
            {
                result.AddError($"Duplicate bucket name '{dn}'");
            }

            // TODO check for duplicate synonyms

            return result;
        }
    }
}
