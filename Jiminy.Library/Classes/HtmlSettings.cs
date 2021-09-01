namespace Jiminy.Classes
{
    public class HtmlSettings
    {
        public bool ShowDiagnostics { get; set; }
        public bool VerboseDiagnostics { get; set; }
        public string? HtmlTemplateFileName { get; set; }
        
        public List<OutputSpecification> Outputs { get; set; } = new();
    }

    public class OutputSpecification
    {
        /// <summary>
        /// Generate this output if true
        /// </summary>
        public bool IsEnabled { get; set; }

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
        /// Items with these tags will be included in this output, names
        /// must match exactly. If this is empty, items with any tags will be included
        /// </summary>
        //public List<string> IncludeTagNames { get; set; } = new();

        /// <summary>
        /// Items with these project names will be included in this output, names
        /// must match exactly. If this is empty, items with any project names will be included
        /// </summary>
        public List<string> IncludeProjectNames { get; set; } = new();
    }
}
