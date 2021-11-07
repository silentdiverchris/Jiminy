using System.Text.RegularExpressions;

namespace Jiminy.Classes
{
    public class BaseDefinition
    {
        /// <summary>
        /// Should be unique, an error will be generated on startup if duplicates are found.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// This is used to display icons in the output, some have a '{value}' portion, which will
        /// be replaced by the text values in enPriority for a priority tag, enDateStatus in the case of 
        /// reminders and due dates, ebBucket for bucket items etc.
        /// On startup, this will be filled out to the full path of the file by combining it with
        /// the MediaDirectoryPath setting.
        /// </summary>
        public string? IconFileName { get; set; } = null;

        /// <summary>
        /// This is the name of a colour that icons and text will be shown in for this priority, eg 'red', 'orange' etc
        /// </summary>
        public string? Colour { get; set; }

        public short DisplayOrder { get; set; } = Constants.DEFAULT_PROJECT_DISPLAY_ORDER;

        /// <summary>
        /// Human description, will be shown as a popup when user floats over the icon, and possibly elsewhere
        /// </summary>
        public string Description { get; set; } = "";

        public Result Validate(string entityName)
        {
            Result result = new();

            if (Name.Length < 2)
            {
                result.AddError($"{entityName} '{Name}' is too short, 2 characters minimum");
            }

            if (new Regex(@"^[a-zA-Z0-9\s,]*$").IsMatch(Name) == false)
            {
                result.AddError($"Name '{entityName}' is invalid, it must be alphanumeric");
            }

            return result;
        }
    }
}
