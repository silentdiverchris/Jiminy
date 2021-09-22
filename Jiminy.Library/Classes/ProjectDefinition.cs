using Jiminy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jiminy.Classes
{
    /// <summary>
    /// Represents a project
    /// </summary>
    public class ProjectDefinition : BaseDefinition
    {
        public short Number { get; set; } = 0;
    }

    /// <summary>
    /// Stores and manages the register of projects known to the system
    /// </summary>
    public class ProjectDefinitionList
    {
        public List<ProjectDefinition> Items { get; set; } = new();

        public bool Exists(string name, bool errorIfNotFound)
        {
            return Get(name: name, allowPartial: false, errorIfNotFound: errorIfNotFound) is not null;
        }

        public ProjectDefinition? Get(string? name, bool allowPartial, bool errorIfNotFound)
        {
            if (name.IsEmpty())
                return null;

            var found = Items.FirstOrDefault(_ => _.Name.ToLower() == name!.ToLower());

            if (found is null && allowPartial)
            {
                found = Items.FirstOrDefault(_ => _.Name.ToLower().StartsWith(name!.ToLower()));
            }

            if (found is null && errorIfNotFound)
            {
                throw new Exception($"ProjectDefinition.Get cannot find project '{name}' ({(allowPartial ? "partial" :"full")}) and this is considered an error");
            }

            return found;
        }

        public ProjectDefinition? GetOrAdd(string name, bool allowPartial, string? description = null, string? iconFileName = null, short displayOrder = Constants.DEFAULT_PROJECT_DISPLAY_ORDER)
        {
            var pd = Get(name, allowPartial: allowPartial, errorIfNotFound: false);

            if (pd is null)
            {
                pd = Add(name: name, description: description, iconFileName: iconFileName, displayOrder: displayOrder);
            }

            return pd;
        }

        public ProjectDefinition Add(string name, string? description = null, string? iconFileName = "project.svg", short displayOrder = Constants.DEFAULT_PROJECT_DISPLAY_ORDER)
        {
            ProjectDefinition? pd = Items.SingleOrDefault(_ => _.Name.ToLower() == name.ToLower());

            if (pd is null)
            {
                if (description.IsEmpty())
                {
                    description = $"Project {name}";
                }

                pd = new ProjectDefinition { Name = name, Description = description!, IconFileName = iconFileName, DisplayOrder = displayOrder };

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
