using Jiminy.Classes;
using Jiminy.Utilities;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Services
{
    internal class FileReaderService : IDisposable
    {
        private readonly AppSettings _appSettings;
        private readonly LogService _logService;

        private List<Item> _tagSets = new();

        public FileReaderService(
            AppSettings appSettings,
            LogService logService)
        {
            _appSettings = appSettings;
            _logService = logService;
        }

        public async Task<Result> ReadFile(MonitoredFile mf)
        {
            Result result = new("FileReaderService.ReadFile");

            if (File.Exists(mf.FullName))
            {
                string[] lines = File.ReadAllLines(mf.FullName);

                _logService.LogToConsole($"Read {lines.Length} lines from {mf.FullName}");

                result.SubsumeResult(await InterpretLines(lines));
            }
            else
            {
                result.AddError($"File {mf.FullName} does not exist");
            }

            foreach (var tagSet in _tagSets)
            {
                tagSet.FullFileName = mf.FullName!;
            }

            return result;
        }

        private async Task<Result> InterpretLines(string[] lines)
        {
            Result result = new("InterpretLines");

            int lineNumber = 0;
            Item? contextTagSet = null;

            foreach (string line in lines) // Don't filter out blank lines here or it breaks line numbering
            {
                lineNumber++;

                if (!string.IsNullOrEmpty(line))
                {
                    Result extractResult = TagSetUtilities.ExtractTagSet(line, _appSettings.MarkdownSettings, out Item? ts);

                    if (result.HasErrorsOrWarnings)
                    {
                        await _logService.ProcessResult(extractResult);
                    }

                    if (ts is not null)
                    {
                        // If we have a context-setting tagset, overwrite unspecified
                        // values in subsequent ones

                        if (ts.IsContext)
                        {
                            ts.Diagnostics.Add("Setting context");

                            contextTagSet = ts;
                        }
                        else if (contextTagSet is not null)
                        {
                            if (ts.ProjectName is null)
                            {
                                ts.Diagnostics.Add($"Applying context project '{contextTagSet.ProjectName}'");
                                ts.ProjectName = contextTagSet.ProjectName;
                            }

                            if (ts.Bucket == enBucket.None)
                            {
                                ts.Diagnostics.Add($"Applying context GTDList '{contextTagSet.Bucket}'");
                                ts.Bucket = contextTagSet.Bucket;
                            }

                            if (contextTagSet.IsDaily)
                            {
                                ts.IsDaily = contextTagSet.IsDaily;
                            }

                            if (contextTagSet.IsWeekly)
                            {
                                ts.IsWeekly = contextTagSet.IsWeekly;
                            }

                            if (contextTagSet.IsMonthly)
                            {
                                ts.IsMonthly = contextTagSet.IsMonthly;
                            }
                        }

                        ts.Warnings = extractResult.TextSummary;
                        ts.LineNumber = lineNumber;
                        _tagSets.Add(ts);
                    }
                }
            }

            //foreach (var tagSet in _tagSets)
            //{
            //    _logService.LogToConsole(tagSet.ToString());
            //}

            return result;
        }

        public List<Item> FoundTagSets => _tagSets;

        public void Dispose()
        {
            // Nothing to do right now
        }
    }
}
