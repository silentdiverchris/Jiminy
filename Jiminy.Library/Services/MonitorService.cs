using Jiminy.Classes;
using Jiminy.Helpers;
using Jiminy.Utilities;
using System.Text.RegularExpressions;
using static Jiminy.Classes.Delegates;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Services
{
    public class MonitorService : IDisposable
    {
#if DEBUG
        private const bool _alwaysCreateAppSettingsOnStartup = true;
#else
        private const bool _alwaysCreateAppSettingsOnStartup = false;
#endif
        private readonly LogService _logService;

        private AppSettings _appSettings;

        private string _appSettingsFileName = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        private List<FileSystemWatcher> _fileSystemWatchers = new();

        private Queue<string> _filesToScanQueue = new();

        private Dictionary<string, MonitoredFile> _monitoredFiles = new();
        private Dictionary<string, MonitoredDirectory> _monitoredDirectories = new();

        private bool _queueChanged = false;
        private bool _regenerationRequired = false;

        private ItemRegistry _itemRegistry = new();

        private List<LogEntry> _recentLogEntries = new();

        private ConsoleDelegate _consoleDelegate;

        public bool IsInitialised => _appSettings.IsValid;

        public MonitorService(ConsoleDelegate consoleDelegate)
        {
            _consoleDelegate = consoleDelegate;

            Result result = LoadConfiguration(out AppSettings? newAppSettings, out _);

            if (result.HasNoErrors && newAppSettings is not null)
            {
                _appSettings = newAppSettings;

                LogEntry log = new($"Initialising Jiminy");

                _recentLogEntries.Add(log);

                _logService = new LogService(appSettings: _appSettings, consoleDelegate: consoleDelegate);
            }
            else
            {
                _appSettings = new();
                _logService = new(_appSettings, _consoleDelegate);
            }

            if (!_appSettings.IsValid)
            {
                result.AddError("Loaded app settings is not valid, canot start");
            }

            if (result.HasErrorsOrWarnings)
            {
                consoleDelegate.Invoke(new LogEntry(result.TextSummary, severity: result.HasErrors ? enSeverity.Error : enSeverity.Warning));
            }
        }

        private Result LoadConfiguration(out AppSettings? newAppSettings, out bool settingsChanged)
        {
            settingsChanged = false;

            Result result = new("LoadConfiguration");
            // Create default appsettings.json if it does not exist, this happens at first run, and if the
            // user deletes or moves it to generate a fresh one that they can customise.

            if (!File.Exists(_appSettingsFileName) || (_alwaysCreateAppSettingsOnStartup && _appSettings is null))
            {
                _consoleDelegate.Invoke(new LogEntry($"Recreating appSettings '{_appSettingsFileName}'"));

                AppSettingsUtilities.CreateDefaultAppSettings(_appSettingsFileName);
            }

            FileInfo fi = new(_appSettingsFileName);

            if (_appSettings is null || fi.LastWriteTimeUtc != _appSettings.SettingsFileDateTime)
            {
                _consoleDelegate.Invoke(new LogEntry($"Loading settings from '{_appSettingsFileName}'"));

                result = AppSettingsUtilities.LoadFile(_appSettingsFileName, out newAppSettings);

                if (newAppSettings is not null)
                {
                    result.SubsumeResult(AppSettingsUtilities.InitialiseAppSettings(newAppSettings));

                    settingsChanged = newAppSettings.IsValid = result.HasNoErrors; 
                }
            }
            else
            {
                newAppSettings = _appSettings;
                //result.AddDebug("Settings file unchanged, not reloading it");
            }

            if (_logService is not null)
            {
                _ = _logService.ProcessResult(result);
            }

            return result;
        }

        public async Task<Result> Run()
        {
            Result result = new("Monitor.Run", true);

            PrefillFilesToScan(_appSettings.MonitoredDirectories.Where(_ => _.IsActive).ToList());

            InitialiseFileSystemWatchers();

            await DoMainLoop();

            return result;
        }

        private void InitialiseFileSystemWatchers()
        {
            LogEntry log = new($"Initialising file watchers");

            _recentLogEntries.Add(log);
            _logService.LogToConsole(log);

            if (_fileSystemWatchers.Any())
            {
                foreach (var fw in _fileSystemWatchers)
                {
                    fw.EnableRaisingEvents = false;
                    fw.Dispose();
                }

                _fileSystemWatchers = new();
            }

            foreach (var dir in _appSettings.MonitoredDirectories.Where(_ => _.Exists && _.IsActive))
            {
                CreateFileSystemWatchers(dir);
            }
        }

        private async Task DoMainLoop()
        {
            bool keepRunning = true;

            while (keepRunning)
            {
                int queueItemCount = _filesToScanQueue.Count;

                if (queueItemCount > 0)
                {
                    _queueChanged = false;

                    while (_filesToScanQueue.Count > 0)
                    {
                        await ReadFile(_filesToScanQueue.Dequeue());
                    }

                    _regenerationRequired = true;
                }

                if (_regenerationRequired)
                {
                    _regenerationRequired = false;

                    Result reloadConfigResult = LoadConfiguration(out AppSettings? newAppSettings, out bool settingsChanged);

                    if (reloadConfigResult.HasNoErrors && newAppSettings is not null)
                    {
                        _appSettings = newAppSettings;

                        if (settingsChanged)
                        {
                            InitialiseFileSystemWatchers();
                        }
                    }
                    else
                    {
                        reloadConfigResult.AddError("Failed to reload configuration, using previous configuration");
                    }

                    await _logService.ProcessResult(reloadConfigResult);

                    Result htmlBuildResult = new();

                    if (File.Exists(_appSettings.OutputSettings.HtmlTemplateFileName))
                    {
                        htmlBuildResult.AddInfo($"Reading template from '{_appSettings.OutputSettings.HtmlTemplateFileName}'");

                        await _logService.ProcessResult(htmlBuildResult);

                        using (var builder = new OutputBuilderService(_appSettings))
                        {
                            htmlBuildResult.SubsumeResult(await builder.BuildOutputs(_itemRegistry, _recentLogEntries));
                        }
                    }
                    else
                    {
                        htmlBuildResult.AddError($"Template '{_appSettings.OutputSettings.HtmlTemplateFileName}' does not exist");
                    }

                    await _logService.ProcessResult(htmlBuildResult);
                }

                int pauseMs = _appSettings.LatencySeconds * 1000;

                for (int i = 1; i < 10; i++)
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
            foreach (var excludeRegex in _appSettings.IgnoredFilesRegexList)
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

                    var log = new LogEntry($"File '{fullFileName}' marked to be read");

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
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;
                watcher.Error += OnError;

                watcher.Filter = dir.IncludeFileSpecification;
                watcher.IncludeSubdirectories = dir.Recursive;
                watcher.EnableRaisingEvents = true;

                _fileSystemWatchers.Add(watcher);
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

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Result result = new("Monitor.OnDeleted");

            _itemRegistry.DeleteItemsFromFile(e.FullPath);

            result.AddInfo($"File '{e.FullPath}' deleted");

            _ = _logService.ProcessResult(result);

            _regenerationRequired = true;
        }

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

        //private MonitoredDirectory GetMonitoredDirectory(string path)
        //{
        //    var md = _monitoredDirectories[path];

        //    if (md is not null)
        //    {
        //        return md;
        //    }
        //    else
        //    {
        //        throw new Exception($"GetMonitoredDirectory failed to find '{path}'");
        //    }
        //}

        //private void MarkFileDeleted(string fullFileName)
        //{
        //    var mf = GetMonitoredFile(fullFileName, false);

        //    if (mf is not null)
        //    {
        //        mf.Exists = false;
        //    }
        //}

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

            _regenerationRequired = true;

            //EnqueueFileToScan(newFullFileName);
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
