namespace Jiminy.Classes
{
    public class Delegates
    {
        /// <summary>
        /// Delegate passed into the services to allow writing to the console
        /// </summary>
        /// <param name="entry"></param>
        public delegate void ConsoleDelegate(LogEntry entry);
    }
}
