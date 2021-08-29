using Jiminy.Classes;
using Jiminy.Utilities;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Services
{

    internal class FileReaderService : IDisposable
    {
        private readonly TagService _tagService;

        //private readonly AppSettings _appSettings;
        private readonly LogService _logService;

        private List<Item> _tagSets = new();

        public FileReaderService(
            AppSettings appSettings,
            LogService logService)
        {
            //_appSettings = appSettings;
            _logService = logService;

            _tagService = new TagService(appSettings);
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
            Item? contextItem = null;

            foreach (string line in lines) // Don't filter out blank lines here or it breaks line numbering
            {
                lineNumber++;

                if (!string.IsNullOrEmpty(line))
                {
                    Result extractResult = _tagService.ExtractTagSet(line, out Item? item);

                    if (result.HasErrorsOrWarnings)
                    {
                        await _logService.ProcessResult(extractResult);
                    }

                    if (item is not null)
                    {
                        // If we have a context-setting tagset, overwrite unspecified
                        // values in subsequent ones

                        if (item.IsContext)
                        {
                            item.Diagnostics.Add("Setting context");

                            contextItem = item;
                        }
                        else if (contextItem is not null)
                        {
                            if (item.ProjectName is null)
                            {
                                item.Diagnostics.Add($"Applying context project '{contextItem.ProjectName}'");
                                item.ProjectName = contextItem.ProjectName;
                            }

                            if (item.BucketName is null)
                            {
                                item.Diagnostics.Add($"Applying bucket '{contextItem.BucketName}'");
                                item.BucketName = contextItem.BucketName;
                            }

                            if (item.Repeat == enRepeat.None)
                            {
                                item.Repeat = contextItem.Repeat;
                            }
                        }

                        item.Warnings = extractResult.TextSummary;
                        item.LineNumber = lineNumber;
                        _tagSets.Add(item);
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
