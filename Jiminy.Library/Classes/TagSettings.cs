namespace Jiminy.Classes
{
    public class TagSettings
    {
        public string Prefix { get; set; } = "=";
        public string Suffix { get; set; } = "=";
        public string Seperator { get; set; } = "-";
        public string Delimiter { get; set; } = ":";

        public TagDefinitionList Defintions { get; set; } = new();

        public Result Validate()
        {
            Result result = new();

            result.SubsumeResult(Defintions.Validate());

            if (Prefix.Length == 0)
            {
                result.AddError($"Prefix '{Prefix}' is invalid, it needs to be at least one character");
            }

            if (Suffix.Length == 0)
            {
                result.AddError($"Suffix '{Suffix}' is invalid, it needs to be at least one character");
            }

            if (Seperator.Length != 1)
            {
                result.AddError($"Separator '{Seperator}' is invalid, it needs to be just one character");
            }

            if (Delimiter.Length != 1)
            {
                result.AddError($"Delimiter '{Delimiter}' is invalid, it needs to be just one character");
            }

            return result;
        }
    }
}
