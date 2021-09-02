using Jiminy.Classes;
using Jiminy.Helpers;
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
                tagSet.SourceFileName = mf.FullName!;
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

                if (line.NotEmpty())
                {
                    Result extractResult = _tagService.ExtractTagSet(line, out Item? item);

                    if (result.HasErrorsOrWarnings)
                    {
                        await _logService.ProcessResult(extractResult);
                    }

                    if (item is not null)
                    {
                        item.FullText = line;

                        // If we have a context-setting tagset, overwrite unspecified
                        // values in subsequent ones

                        if (item.SetsContext)
                        {
                            item.Diagnostics.Add("Setting context");

                            contextItem = item;
                        }
                        else if (item.ClearsContext)
                        {
                            item.Diagnostics.Add("Clearing context");

                            contextItem = null;
                        }
                        else if (contextItem is not null)
                        {
                            ApplyContext(item, contextItem);
                        }

                        if (extractResult.TextSummary.NotEmpty())
                        {
                            item.Warnings.Add(extractResult.TextSummary);
                        }

                        item.SourceLineNumber = lineNumber;

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

        private void ApplyContext(Item item, Item contextItem)
        {
            if (item.Project is null && contextItem.Project is not null)
            {
                item.AddTagInstance(contextItem.Project, true);
            }

            if (item.BucketName is null && contextItem.Bucket is not null)
            {
                item.AddTagInstance(contextItem.Bucket, true);
            }

            if (item.PriorityNumber is null && contextItem.Priority is not null)
            {
                item.AddTagInstance(contextItem.Priority, true);
            }

            if (item.RepeatName is null && contextItem.Repeat is not null)
            {
                item.AddTagInstance(contextItem.Repeat, true);

                var ti = contextItem.GetTagInstance(enTagType.Repeating); 

                if (ti is not null)
                {
                    item.AddTagInstance(ti, true);
                }
            }

            foreach (var contextCustomTag in contextItem.TagInstances.Where(_ => _.Type == enTagType.Custom))
            {
                if (!item.TagInstances.Any(_ => _.Type == contextCustomTag.Type))
                {
                    item.AddTagInstance(contextCustomTag, true);
                }
            }
        }

        public List<Item> FoundTagSets => _tagSets;

        public void Dispose()
        {
            // Nothing to do right now
        }
    }
}
