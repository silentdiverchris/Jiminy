using Jiminy.Classes;
using Jiminy.Helpers;
using Jiminy.Utilities;
using System.Text.RegularExpressions;
using static Jiminy.Classes.Delegates;

namespace Jiminy.Services
{
    public class MonitorService : IDisposable
    {
        private readonly LogService _logService;

        private readonly AppSettings _appSettings;

        private List<FileSystemWatcher> _fileWatchers = new();

        private Queue<string> _filesToScanQueue = new Queue<string>();

        private Dictionary<string, MonitoredFile> _monitoredFiles = new();
        private Dictionary<string, MonitoredDirectory> _monitoredDirectories = new();

        private bool _queueChanged = false;

        private ItemRegistry _itemRegistry = new();

        private List<Regex> _ignoredFilesRegexList = new();

        private List<LogEntry> _recentLogEntries = new();

        public MonitorService(AppSettings appSettings, ConsoleDelegate consoleDelegate)
        {
            LogEntry log = new($"Initialising Jiminy");
            _recentLogEntries.Add(log);

            _appSettings = appSettings;

            _logService = new LogService(
                appSettings: appSettings,
                consoleDelegate: consoleDelegate);
        }

        public async Task<Result> Run()
        {
            Result result = new("Monitor.Run", true);

            _appSettings.MarkdownSettings.TagDelimiterSets = new List<ItemDelimiterSet>
            {
                new ItemDelimiterSet("=", "=", "-", ":")
            };

            result = AppSettingsUtilities.ValidateAppSettings(_appSettings);

            await _logService.ProcessResult(result);

            if (result.HasNoErrors)
            {
                foreach (var ignoreSpec in _appSettings.IgnoreFileSpecifications)
                {
                    _ignoredFilesRegexList.Add(ignoreSpec.GenerateRegexForFileMask());
                }

                PrefillFilesToScan(_appSettings.MonitoredDirectories);

                await DoMainLoop();
            }

            return result;
        }

        private async Task DoMainLoop()
        {
            bool initialising = true;
            bool keepRunning = true;

            while (keepRunning)
            {
                //_logService.LogToConsole(new LogEntry("Checking queue"));

                bool newScansDone = false;
                int queueItemCount = _filesToScanQueue.Count;

                if (queueItemCount > 0)
                {
                    _queueChanged = false;

                    //LogEntry log = new($"Queue has {queueItemCount} item{IntHelpers.PluralSuffix(queueItemCount)}");

                    //_recentLogEntries.Add(log);
                    //_logService.LogToConsole(log);

                    if (!initialising && !newScansDone)
                    {
                        // Tiny pause to allow a source file to be released by
                        // something that might still be hanging on to it

                        Thread.Sleep(500);
                    }

                    while (_filesToScanQueue.Count > 0)
                    {
                        string fileName = _filesToScanQueue.Dequeue();

                        await ReadFile(fileName);

                        newScansDone = true;
                    }
                }

                if (initialising)
                {
                    LogEntry log = new($"Initialising file watchers");

                    _recentLogEntries.Add(log);
                    _logService.LogToConsole(log);

                    foreach (var dir in _appSettings.MonitoredDirectories.Where(_ => _.Exists))
                    {
                        CreateFileSystemWatchers(dir);
                    }

                    initialising = false;
                }

                if (newScansDone)
                {
                    string htmlTemplateFileName = _appSettings.HtmlSettings.HtmlTemplateFileName.Contains(Path.DirectorySeparatorChar)
                        ? _appSettings.HtmlSettings.HtmlTemplateFileName
                        : Path.Join(AppDomain.CurrentDomain.BaseDirectory, _appSettings.HtmlSettings.HtmlTemplateFileName);

                    string htmlOutputFileName = _appSettings.HtmlSettings.HtmlOutputFileName.Contains(Path.DirectorySeparatorChar)
                        ? _appSettings.HtmlSettings.HtmlOutputFileName
                        : Path.Join(AppDomain.CurrentDomain.BaseDirectory, _appSettings.HtmlSettings.HtmlOutputFileName);

                    Result htmlBuildResult = new();

                    if (File.Exists(htmlTemplateFileName))
                    {
                        htmlBuildResult.AddInfo($"Reading template from '{htmlTemplateFileName}'");
                        await _logService.ProcessResult(htmlBuildResult);

                        htmlBuildResult.SubsumeResult(await HtmlBuilderUtilities.BuildHtmlPage(_appSettings, _itemRegistry, _recentLogEntries, htmlTemplateFileName, htmlOutputFileName));

                        if (htmlBuildResult.HasNoErrors)
                        {
                            htmlBuildResult.AddSuccess($"{DateTime.Now.ToString(Constants.DATE_FORMAT_TIME_ONLY_SECONDS)} Refreshed output '{_appSettings.HtmlSettings.HtmlOutputFileName}'");
                        }
                    }
                    else
                    {
                        htmlBuildResult.AddError($"Template '{htmlTemplateFileName}' does not exist");
                    }

                    await _logService.ProcessResult(htmlBuildResult);
                }

#if DEBUG
                //int pauseMs = 1000;
                int pauseMs = _appSettings.LatencySeconds * 1000;
#else
                int pauseMs = _appSettings.LatencySeconds * 1000;
#endif
                for (int i = 1; i < 10 ; i++)
                {
                    Thread.Sleep(pauseMs / 10);

                    if (_queueChanged)
                        break;
                }
            }
        }

        private void PrefillFilesToScan(List<MonitoredDirectory> dirList)
        {
            foreach (var dir in dirList)
            {
                LogEntry log = new($"Scanning '{dir.Path}'");

                _recentLogEntries.Add(log);
                _logService.LogToConsole(log);

                PrefillFilesToScan(dir);
            }
        }

        private void PrefillFilesToScan(MonitoredDirectory dir)
        {
            var options = dir.Recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            foreach (var fullFileName in Directory.GetFiles(dir.Path, "*.md", searchOption: options))
            {
                if (!FileIsIgnorable(fullFileName))
                {
                    //LogEntry log = new($"Prefilling from '{fullFileName}'");

                    //_recentLogEntries.Add(log);
                    //_logService.LogToConsole(log);

                    EnqueueFileToScan(fullFileName);
                }
            }
        }

        private bool FileIsIgnorable(string fullFileName)
        {
            foreach (var excludeRegex in _ignoredFilesRegexList)
            {
                if (excludeRegex.IsMatch(fullFileName))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnqueueFileToScan(string fullFileName)
        {
            if (FileIsIgnorable(fullFileName))
            {
                return;
            }

            if (File.Exists(fullFileName))
            {
                if (!_monitoredFiles.ContainsKey(fullFileName))
                {
                    RegisterNewFile(fullFileName);
                }

                if (!_filesToScanQueue.Contains(fullFileName))
                {
                    _filesToScanQueue.Enqueue(fullFileName);

                    var log = new LogEntry($"File '{fullFileName}' enqueued");

                    _recentLogEntries.Add(log);
                    _logService.LogToConsole(log);

                    _queueChanged = true;
                }
            }
        }

        private void CreateFileSystemWatchers(MonitoredDirectory dir)
        {
            if (dir.Path is not null)
            {
                var watcher = new FileSystemWatcher(dir.Path)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
                };

                watcher.Changed += OnChanged;
                //watcher.Created += OnCreated;
                //watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;
                watcher.Error += OnError;

                watcher.Filter = dir.IncludeFileSpecification;
                watcher.IncludeSubdirectories = dir.Recursive;
                watcher.EnableRaisingEvents = true;

                _fileWatchers.Add(watcher);
            }
            else
            {
                throw new Exception($"CreateFileSystemWatchers null path");
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Result result = new("Monitor.OnError");

            result.AddException(e.GetException());

            _ = _logService.ProcessResult(result);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Result result = new("Monitor.OnRenamed");

            string msg = $"File '{e.OldFullPath}' renamed to '{e.Name}'";
            result.AddInfo(msg);

            _recentLogEntries.Add(new LogEntry(msg));

            RenameFile(e.OldFullPath, e.FullPath);

            _ = _logService.ProcessResult(result);
        }

        //private void OnDeleted(object sender, FileSystemEventArgs e)
        //{
        //    Result result = new("Monitor.OnDeleted");

        //    MarkFileDeleted(e.FullPath);

        //    result.AddInfo($"File '{e.FullPath}' deleted");

        //    _ = _logService.ProcessResult(result);
        //}

        //private void OnCreated(object sender, FileSystemEventArgs e)
        //{
        //    Result result = new("Monitor.OnCreated");

        //    RegisterNewFile(e.FullPath);

        //    AddToScanQueue(e.FullPath);

        //    result.AddInfo($"File '{e.FullPath}' created");

        //    _ = _logService.ProcessResult(result);
        //}

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            //Result result = new("Monitor.OnChanged");

            //_logService.LogToConsole(new LogEntry($"File '{e.FullPath}' change event"));

            //_recentLogEntries.Add(new LogEntry($"File '{e.FullPath}' updated"));

            EnqueueFileToScan(e.FullPath);
        }

        private async Task ReadFile(string fullFileName)
        {
            Result result = new("Monitor.ScanFile");

            result.AddInfo($"Reading file '{fullFileName}'");

            _recentLogEntries.Add(new LogEntry($"File '{fullFileName}' read"));

            _ = _logService.ProcessResult(result);

            var mf = GetMonitoredFile(fullFileName, true);

            using (var fileReaderService = new FileReaderService(_appSettings, _logService))
            {
                result.SubsumeResult(await fileReaderService.ReadFile(mf));

                _itemRegistry.AddRange(fileReaderService.FoundTagSets, fullFileName);
            }

            if (result.HasErrors)
            {
                result.AddInfo($"Errors reading '{fullFileName}'");
            }
            else if (result.HasWarnings)
            {
                result.AddInfo($"Warnings reading '{fullFileName}'");
            }
            else
            {
                result.AddInfo($"Success reading '{fullFileName}'");
            }

            _ = _logService.ProcessResult(result);

            SetFileScanned(fullFileName);
        }

        private MonitoredFile GetMonitoredFile(string fullFileName, bool registerIfNotFound)
        {
            if (_monitoredFiles.ContainsKey(fullFileName))
            {
                var mf = _monitoredFiles[fullFileName];
                return mf;
            }
            else
            {
                if (registerIfNotFound)
                {
                    var mf = RegisterNewFile(fullFileName);
                    return mf;
                }
                else
                {
                    throw new Exception($"GetMonitoredFile failed to find '{fullFileName}' and was told not to register it");
                }
            }
        }

        private MonitoredDirectory GetMonitoredDirectory(string path)
        {
            var md = _monitoredDirectories[path];

            if (md is not null)
            {
                return md;
            }
            else
            {
                throw new Exception($"GetMonitoredDirectory failed to find '{path}'");
            }
        }

        private void MarkFileDeleted(string fullFileName)
        {
            var mf = GetMonitoredFile(fullFileName, false);

            if (mf is not null)
            {
                mf.Exists = false;
            }
        }

        private void SetFileScanned(string fullFileName)
        {
            var mf = GetMonitoredFile(fullFileName, false);

            if (mf is not null)
            {
                mf.LastScanUtc = DateTime.UtcNow;
            }
            else
            {
                throw new ArgumentException($"SetFileScanned found '{fullFileName}' is not registered");
            }
        }

        private void RenameFile(string oldFullFileName, string newFullFileName)
        {
            if (_monitoredFiles.ContainsKey(oldFullFileName))
            {
                _monitoredFiles.Remove(oldFullFileName);
            }

            if (_monitoredFiles.ContainsKey(newFullFileName))
            {
                _monitoredFiles.Remove(oldFullFileName);
            }

            EnqueueFileToScan(newFullFileName);
        }

        private MonitoredFile RegisterNewFile(string fullFileName)
        {
            if (File.Exists(fullFileName))
            {
                if (!_monitoredFiles.ContainsKey(fullFileName))
                {
                    MonitoredFile mf = new(fullFileName);

                    _monitoredFiles.Add(fullFileName, new MonitoredFile(fullFileName));

                    return mf;
                }
                else
                {
                    throw new ArgumentException($"RegisterNewFile found '{fullFileName}' already registered");
                }
            }
            else
            {
                throw new ArgumentException($"RegisterNewFile found '{fullFileName}' does not exist, cannot register a file that doesn't exist");
            }
        }

        public void Dispose()
        {
            if (_logService is not null)
                _logService.Dispose();
        }
    }
}
