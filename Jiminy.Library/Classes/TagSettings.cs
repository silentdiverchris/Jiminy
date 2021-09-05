namespace Jiminy.Classes
{
    public class TagSettings
    {
        public string Prefix { get; set; } = "=";
        public string Suffix { get; set; } = "=";
        public string Separator { get; set; } = "-";
        public string Delimiter { get; set; } = ":";
        public string FromHere { get; set; } = ">";
        public string ToHere { get; set; } = "<=";

        public TagDefinitionList Definitions { get; set; } = new();

        public Result Validate()
        {
            Result result = new();

            result.SubsumeResult(Definitions.Validate());

            if (Prefix.Length == 0)
            {
                result.AddError($"Prefix '{Prefix}' is invalid, it needs to be at least one character");
            }

            if (Suffix.Length == 0)
            {
                result.AddError($"Suffix '{Suffix}' is invalid, it needs to be at least one character");
            }

            if (Separator.Length != 1)
            {
                result.AddError($"Separator '{Separator}' is invalid, it needs to be just one character");
            }

            if (Delimiter.Length != 1)
            {
                result.AddError($"Delimiter '{Delimiter}' is invalid, it needs to be just one character");
            }

            return result;
        }
    }
}
