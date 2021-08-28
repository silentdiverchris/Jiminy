namespace Jiminy.Classes
{
    public class TagSettings
    {
        public List<TagDefinition> Tags { get; set; } = new();
    }

    public class TagDefinition
    {
        public bool GenerateView { get; set; } = false;
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconFileName { get; set; } = "";
    }
}
