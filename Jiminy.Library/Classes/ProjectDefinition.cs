using Jiminy.Helpers;
using System.Text.RegularExpressions;

namespace Jiminy.Classes
{
    public class ProjectDefinition : BaseDefinition
    {
        public short Number { get; set; } = 0;
    }

    public class ProjectDefinitionList
    {
        public List<ProjectDefinition> Items { get; set; } = new();

        public bool Exists(string name)
        {
            return Get(name) is not null;
        }

        public ProjectDefinition? Get(string? name, bool allowPartial = false)
        {
            if (name.IsEmpty())
                return null;

            var found = Items.FirstOrDefault(_ => _.Name.ToLower() == name);

            if (found is null && allowPartial)
            {
                found = Items.FirstOrDefault(_ => _.Name.ToLower().StartsWith(name!));
            }

            return found;
        }

        public ProjectDefinition? GetOrAdd(string name, string? description = null, string? iconFileName = null)
        {
            var pd = Get(name, allowPartial: false);

            if (pd is null)
            {
                pd = Add(name, description, iconFileName);
            }

            return pd;
        }

        public ProjectDefinition? Get(int number) => Items.FirstOrDefault(_ => _.Number == number);

        public ProjectDefinition Add(string name, string? description = null, string? iconFileName = "project.svg")
        {
            ProjectDefinition? pd = Items.SingleOrDefault(_ => _.Name.ToLower() == name.ToLower());

            if (pd is null)
            {
                if (description.IsEmpty())
                {
                    description = $"Project {name}";
                }

                pd = new ProjectDefinition { Name = name, Description = description!, IconFileName = iconFileName };

                Items.Add(pd);
            }

            return pd;
        }

        public Result Validate()
        {
            Result result = new();

            foreach (var bu in Items)
            {
                result.SubsumeResult(bu.Validate("Project"));
            }

            var duplicateNames = Items.GroupBy(_ => _.Name).Where(g => g.Count() > 1).Select(y => y).ToList();

            foreach (var dn in duplicateNames)
            {
                result.AddError($"Duplicate project name '{dn}'");
            }

            return result;
        }
    }
}
