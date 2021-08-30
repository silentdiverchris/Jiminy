﻿namespace Jiminy.Classes
{
    public class RepeatDefinition : BaseDefinition
    {
        // The total of the four NumberOf... is used - though no functionality for this exists yet

        public int NumberOfDays { get; set; } = 0;
        public int NumberOfWeeks { get; set; } = 0;
        public int NumberOfMonths { get; set; } = 0;
        public int NumberOfYears { get; set; } = 0;
    }

    public class RepeatDefinitionList
    {
        public List<RepeatDefinition> Repeats { get; set; } = new();

        public bool Exists(string name)
        {
            return Get(name) is not null;
        }

        public RepeatDefinition? Get(string? name, bool allowPartial = false)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var found = Repeats.FirstOrDefault(_ => _.Name.ToLower() == name);

            if (found is null && allowPartial)
            {
                found = Repeats.FirstOrDefault(_ => _.Name.ToLower().StartsWith(name));
            }

            return found;
        }

        public Result Validate()
        {
            Result result = new();

            foreach (var bu in Repeats)
            {
                result.SubsumeResult(bu.Validate("Repeat"));
            }

            var duplicateNames = Repeats.GroupBy(_ => _.Name).Where(g => g.Count() > 1).Select(y => y).ToList();

            foreach (var dn in duplicateNames)
            {
                result.AddError($"Duplicate Repeat name '{dn}'");
            }

            return result;
        }
    }
}
