﻿namespace Jiminy.Classes
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
            SetContext = 8,
            Completed = 9,
            Link = 10,
            ClearContext = 11
        }

        /// <summary>
        /// For reminders and due dates
        /// </summary>
        public enum enDateStatus
        {
            Overdue = 1,
            Today = 2,
            Soon = 3,
            Future = 4,
            NoDate = 99
        }

        public enum enSeverity
        {
            Debug = 0,
            Info = 1,
            Success = 2,
            Warning = 3,
            Error = 4,
        }
    }
}
