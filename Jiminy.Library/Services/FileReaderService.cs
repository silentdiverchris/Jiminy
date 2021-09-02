﻿using Jiminy.Classes;
using Jiminy.Helpers;
using Jiminy.Utilities;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Services
{

    internal class FileReaderService : IDisposable
    {
        private readonly TagService _tagService;

        private readonly AppSettings _appSettings;
        private readonly LogService _logService;

        private List<Item> _tagSets = new();

        public FileReaderService(
            AppSettings appSettings,
            LogService logService)
        {
            _appSettings = appSettings;
            _logService = logService;

            _tagService = new TagService(appSettings);
        }

        public async Task<Result> ReadFile(MonitoredFile mf)
        {
            Result result = new("FileReaderService.ReadFile");

            if (File.Exists(mf.FullName))
            {
                FileContent fileContent = new(mf.FullName);

                _logService.LogToConsole($"Read {fileContent.LineCount} lines from {fileContent.FullFileName}");

                result.SubsumeResult(await InterpretFileContent(fileContent));
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

        private async Task<Result> InterpretFileContent(FileContent fileContent)
        {
            Result result = new("InterpretLines");

            Item? contextItem = null;

            FileLine? fileLine = fileContent.GetNextLine(skipEmptyLines: true);

            while (fileLine is not null)
            {
                Result extractResult = _tagService.InterpretLineContent(fileLine, out Item? item);

                if (item is not null)
                {
                    if (item.IncludeSubsequentLines)
                    {
                        item.AssociatedText = fileContent.ConcatenateSubsequentLines(untilMarker: _appSettings.TagSettings.ToHere, prefix: "<div>", linePrefix: "<div>", lineSuffix: "</div>", suffix: "</div>");
                    }

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

                    _tagSets.Add(item);
                }

                await _logService.ProcessResult(extractResult);

                fileLine = fileContent.GetNextLine(skipEmptyLines: true);
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
