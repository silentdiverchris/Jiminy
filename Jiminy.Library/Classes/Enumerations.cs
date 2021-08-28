namespace Jiminy.Classes
{
    public static class Enumerations
    {
        public enum enIcon
        {
            PriorityHigh = 1,
            PriorityMedium = 2,
            PriorityLow = 3,
            SourceFile = 4,
            Reminder = 5,
            Due = 6,
            BucketIn = 7,
            BucketNext = 8,
            BucketWaiting = 9,
            BucketSomeday = 10
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

        public enum enBucket
        {
            None = 0,
            In = 1,
            Next = 2,
            Waiting = 3,
            Someday = 4
        }
    }
}
