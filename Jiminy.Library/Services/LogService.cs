using Jiminy.Classes;
using Jiminy.Helpers;
using Jiminy.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static Jiminy.Classes.Delegates;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Services
{
    /// <summary>
    /// Handles logging to text file, event log and console
    /// </summary>
    internal class LogService : IDisposable
    {
        private readonly LogSettings _logSettings;

        private readonly bool _logToFile = false;
        private readonly bool _logToConsole = true; // No way to turn this off at present

        public bool LoggingToFile => _logToFile;
        public bool LoggingToConsole => _logToConsole;

        private readonly string? _logFileName = null;

        private readonly ConsoleDelegate _consoleDelegate;

        internal string? LogFileName => _logFileName;

        internal LogService(AppSettings appSettings, ConsoleDelegate consoleDelegate)
        {
            _consoleDelegate = consoleDelegate;
            _logSettings = appSettings.LogSettings;

            if (_logSettings.LogDirectoryPath.NotEmpty())
            {
                // If just the directory name, we will asume it's under the install
                // directory, if not, then use the path supplied

                _logSettings.LogDirectoryPath = _logSettings.LogDirectoryPath!.Contains(Path.DirectorySeparatorChar)
                    ? _logSettings.LogDirectoryPath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _logSettings.LogDirectoryPath);

                if (!Directory.Exists(_logSettings.LogDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(_logSettings.LogDirectoryPath);

                        // Allow autheticated users to write to the log directory

                        var Info = new DirectoryInfo(_logSettings.LogDirectoryPath);

#pragma warning disable CA1416 // Validate platform compatibility

                        var Security = Info.GetAccessControl(AccessControlSections.Access);

                        Security.AddAccessRule(
                            rule: new FileSystemAccessRule(
                                identity: "Authenticated Users",
                                fileSystemRights: FileSystemRights.Modify,
                                inheritanceFlags: InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                propagationFlags: PropagationFlags.InheritOnly,
                                type: AccessControlType.Allow));

#pragma warning restore CA1416 // Validate platform compatibility

                    }
                    catch (Exception ex)
                    {
                        string msg = $"Log directory '{_logSettings.LogDirectoryPath}' does not exist and cannot be created, remove the setting or ensure it refers to an existing directory or one that can be created.";
                        EventLogUtilities.WriteEntry(msg, enSeverity.Error);
                        throw new Exception(msg, ex);
                    }
                }

                if (Directory.Exists(_logSettings.LogDirectoryPath))
                {
                    _logFileName = Path.Combine(_logSettings.LogDirectoryPath, $"Jiminy-{DateTime.UtcNow.ToString(Constants.DATE_FORMAT_DATE_TIME_YYYYMMDDHHMMSS)}.log");
                    _logToFile = true;
                }

                if (_logToFile)
                {
                    _consoleDelegate.Invoke(new LogEntry($"Logging to file '{_logFileName}'"));
                }
                else
                {
                    _consoleDelegate.Invoke(new LogEntry($"Not logging to file"));
                }
            }
        }

        /// <summary>
        /// Send each unprocessed message in the result to the configured log destinations
        /// </summary>
        /// <param name="result">The result</param>
        /// <param name="addCompletionItem">To be called at the end of a function, or functional unit, it logs error and warning counts or success</param>
        /// <param name="reportItemCounts">Logs the figures in ItemsFound, ItemsProcessed and BytesProcessed</param>
        /// <returns></returns>
        internal async Task ProcessResult(
            Result result,
            bool addCompletionItem = false)
        {
            foreach (var msg in result.UnprocessedMessages.OrderBy(_ => _.CreatedUtc))
            {
                await AddLogAsync(
                    new LogEntry(
                        logText: msg.Text,
                        severity: msg.Severity,
                        createdUtc: msg.CreatedUtc));
            }
                        
            if (addCompletionItem)
            {
                if (result.HasErrors)
                {
                    result.AddError($"{result.FunctionName} completed with errors");
                }
                else if (result.HasWarnings)
                {
                    result.AddWarning($"{result.FunctionName} completed with warnings");
                }
                else
                {
                    result.AddSuccess($"{result.FunctionName} completed OK");
                }
            }

            result.MarkMessagesWritten();
        }

        private async Task AddLogAsync(LogEntry entry)
        {
            if (_logSettings.VerboseEventLog || entry.AlwaysWriteToEventLog || entry.Severity == enSeverity.Warning || entry.Severity == enSeverity.Error)
            {
                if (entry.Text is not null)
                {
                    EventLogUtilities.WriteEntry(entry.Text, severity: entry.Severity);
                }
            }

            if (_logToConsole && (_logSettings.VerboseConsole || entry.Severity != enSeverity.Debug))
            {
                _consoleDelegate.Invoke(entry);
            }

            if (_logToFile)
            {
                await AddLogToFileAsync(entry);
            }
        }

        private async Task AddLogToFileAsync(LogEntry entry)
        {
            if (_logToFile && _logFileName is not null)
            {
                using (FileStream sourceStream = new(_logFileName, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    byte[] encodedText = Encoding.Unicode.GetBytes(entry.FormatForFile());
                    await sourceStream.WriteAsync(encodedText);
                };
            }
        }

        public void Dispose()
        {
            // We open a connection on demand each time so nothing to do here, though that might change, either way it's nice to be asked.
        }

        internal void LogToConsole(LogEntry entry)
        {
            if (_logToConsole)
            {
                _consoleDelegate.Invoke(entry);
            }
        }

        internal void LogToConsole(string text)
        {
            if (_logToConsole)
            {
                _consoleDelegate.Invoke(new LogEntry(text));
            }
        }
    }
}
