namespace Jiminy.Classes
{
    public static class Enumerations
    {
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
