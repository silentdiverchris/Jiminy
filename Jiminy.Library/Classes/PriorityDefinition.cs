using Jiminy.Helpers;
using System.Text.RegularExpressions;

namespace Jiminy.Classes
{
    public class PriorityDefinition : BaseDefinition
    {
        public short Number { get; set; } = 0;
    }

    public class PriorityDefinitionList
    {
        public List<PriorityDefinition> Priorities { get; set; } = new();

        public bool Exists(string name)
        {
            return Get(name) is not null;
        }

        public PriorityDefinition? Get(string? name, bool allowPartial = false)
        {
            if (name.IsEmpty())
                return null;

            var found = Priorities.FirstOrDefault(_ => _.Name.ToLower() == name);

            if (found is null && allowPartial)
            {
                found = Priorities.FirstOrDefault(_ => _.Name.ToLower().StartsWith(name!));
            }

            return found;
        }

        public PriorityDefinition? Get(int number) => Priorities.FirstOrDefault(_ => _.Number == number);

        public Result Validate()
        {
            Result result = new();

            foreach (var bu in Priorities)
            {
                result.SubsumeResult(bu.Validate("Priority"));
            }

            var duplicateNames = Priorities.GroupBy(_ => _.Name).Where(g => g.Count() > 1).Select(y => y).ToList();

            foreach (var dn in duplicateNames)
            {
                result.AddError($"Duplicate priority name '{dn}'");
            }

            return result;
        }
    }
}
