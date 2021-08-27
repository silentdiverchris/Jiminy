namespace Jiminy.Classes
{
    public class ItemDelimiterSet
    {
        public ItemDelimiterSet(string startDelim, string endDelim, string sepDelim, string qualDelim)
        {
            StartDelim = startDelim;
            EndDelim = endDelim;
            SepDelim = sepDelim;
            QualifierDelim = qualDelim;
        }

        /// <summary>
        /// Strings that indicate the start of a tag, when the system writes tags, the
        /// first in the list will be used.
        /// </summary>
        public string StartDelim { get; set; } = "@";

        /// <summary>
        /// Strings that terminate a tag, a space or CrLf will do the job too. When the 
        /// system writes tags, the first in the list will be used.
        /// </summary>
        public string EndDelim { get; set; } = "@";

        /// <summary>
        /// The delimiter that separates tags, when the system writes tags, the
        /// first in the list will be used.
        /// </summary>
        public string SepDelim { get; set; } = "-";

        /// <summary>
        /// A delimiter between things like project and the project 
        /// name, or reminder and the date/time
        /// </summary>
        public string QualifierDelim { get; set; } = ":";
    }
}
