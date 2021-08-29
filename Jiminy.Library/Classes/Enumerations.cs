namespace Jiminy.Classes
{
    public static class Enumerations
    {
        public enum enTagType
        {
            Custom = 1,
            Priority = 2,
            Reminder = 3,
            Due = 4,
            Bucket = 5,
            Project = 6,
            Repeating = 7,
            Context = 8,
            Completed = 9
        }

        public enum enRepeat
        {
            None = 0,
            Daily = 1,
            Weekly = 2,
            Monthly = 3
        }

        /// <summary>
        /// For reminders and due dates
        /// </summary>
        public enum enDateStatus
        {
            None = 0,
            Overdue = 1,
            Due = 2,
            Imminent = 3,
            Future = 4
        }

        public enum enSeverity
        {
            Debug = 0,
            Info = 1,
            Success = 2,
            Warning = 3,
            Error = 4,
        }

        public enum enPriority
        {
            High = 1,
            Medium = 2,
            Low = 3
        }
    }
}
