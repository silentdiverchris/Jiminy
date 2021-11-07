using System.Collections.Generic;
using System.Linq;

namespace Jiminy.Classes
{
    public class OutputSettings
    {
        public bool ShowDiagnostics { get; set; }
        public bool VerboseDiagnostics { get; set; }
        public string? HtmlTemplateFileName { get; set; }

        /// <summary>
        /// Create empty tabs for projects with no items
        /// </summary>
        public bool CreateEmptyTabs { get; set; }

        public List<OutputSpecification> Outputs { get; set; } = new();
    }

    public class OutputSpecification
    {
        /// <summary>
        /// Generate this output if true
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The title shown at the top of the content, if there is no title here
        /// then the output will still be generated but no title will be shown.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The path that an HTML output file will be written to, if this is 
        /// not supplied, no HTML file will be created
        /// </summary>
        public string? HtmlPath { get; set; }

        /// <summary>
        /// The path that a JSON output file will be written to, if this is 
        /// not supplied, no JSON file will be created
        /// </summary>
        public string? JsonPath { get; set; }

        /// <summary>
        /// Allows a different template to be used for this output
        /// </summary>
        public string? OverrideHtmlTemplateFileName { get; set; }

        /// <summary>
        /// Selects which items from the whole set should go to this
        /// output, if there is nothing here, all items will be used
        /// </summary>
        public ItemSelection? ItemSelection { get; set; } = null;
    }

    public class ItemSelection
    {
        /// <summary>
        /// If this is true, an item must satisfy all of the fiolters, otherwise 
        /// it will be included if it matches any of the filters.
        /// </summary>
        public bool MustMatchAll { get; set; }

        /// <summary>
        /// Items with these tags will be included in this output, names
        /// must match exactly. If this is empty, items with any tags will be included
        /// </summary>
        public List<string> IncludeTagNames { get; set; } = new();

        /// <summary>
        /// Items with these project names will be included in this output, names
        /// must match exactly. If this is empty, items with any project names will be included
        /// </summary>
        public List<string> IncludeProjectNames { get; set; } = new();

        public string GenerateTextDescription()
        {
            if (IncludeProjectNames.Any() || IncludeTagNames.Any())
            {
                string? projectInc = IncludeProjectNames.Any()
                    ? $"projects '{string.Join(", ", IncludeProjectNames)}'"
                    : null;

                string? tagInc = IncludeTagNames.Any()
                    ? $"{(projectInc is null ? null : " and ")}tags '{string.Join(", ", IncludeTagNames)}'"
                    : null;

                return $"Including items that match {(MustMatchAll ? "all" : "any")} of {projectInc}{tagInc}";
            }
            else
            {
                return "Including all items.";
            }
        }
    }
}
