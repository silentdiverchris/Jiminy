using Jiminy.Classes;
using Jiminy.Helpers;
using Jiminy.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Services
{
    /// <summary>
    /// Handles the reading of source files and digging the items and their tags from the contant
    /// </summary>
    internal class FileReaderService : IDisposable
    {
        private readonly TagService _tagService;

        private readonly AppSettings _appSettings;
        private readonly LogService _logService;

        private readonly List<Item> _tagSets = new();

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

            if (mf.FullName.IsExistingFileName())
            {
                FileContent fileContent = new(mf.FullName!);

                _logService.LogToConsole($"Read {fileContent.LineCount} lines from {fileContent.FullFileName}");

                result.SubsumeResult(await InterpretFileContent(fileContent));
            }
            else
            {
                result.AddError($"File '{mf.FullName}' does not exist");
            }

            foreach (var tagSet in _tagSets)
            {
                tagSet.SourceFileName = mf.FullName!;
            }

            return result;
        }

        /// <summary>
        /// The main function that extracts items from source files via the FileContent abstraction
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
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
                    item.SourceLineNumber = fileLine.LineNumber;
                    item.Id = GenerateItemId(fileContent.FullFileName, fileLine.LineNumber);

                    if (item.IncludeSubsequentLines)
                    {
                        item.AssociatedText = fileContent.ConcatenateSubsequentLines(untilMarker: _appSettings.TagSettings.ToHere, prefix: "<div>", linePrefix: "<div>", lineSuffix: "</div>", suffix: "</div>");
                    }

                    if (item.SetsContext)
                    {
                        item.Diagnostics.Add("Setting context");

                        contextItem = UpdateContext(contextItem, item);
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
                    else if (!item.TagInstances.Any())
                    {
                        item.Warnings.Add($"Item has no inline tags or pre-defined context");
                    }

                    if (extractResult.Messages.Any())
                    {
                        item.Warnings.AddRange(extractResult.Messages.Select(_ => _.Text));
                    }

                    if (!item.ClearsContext && !item.SetsContext)
                    {
                        _tagService.ApplyMissingTags(item);
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

        private Item UpdateContext(Item? contextItem, Item item)
        {
            if (contextItem is null)
            {
                return item;
            }
            else
            {
                foreach (var ti in item.TagInstances)
                {
                    var existing = contextItem.TagInstances.SingleOrDefault(_ => _.DefinitionName == ti.DefinitionName);

                    if (existing is not null)
                    {
                        contextItem.RemoveTagInstance(ti.DefinitionName);
                    }

                    contextItem.AddTagInstance(ti);
                }
            }

            return contextItem;
        }

        private string? GenerateItemId(string? fullFileName, int lineNumber)
        {
            return Math.Abs((fullFileName + lineNumber.ToString()).GetHashCode()).ToString();
        }

        private void ApplyContext(Item item, Item contextItem)
        {
            if (item.ProjectTag is null && contextItem.ProjectTag is not null)
            {
                item.AddTagInstance(contextItem.ProjectTag, true);
            }

            if (item.BucketName is null && contextItem.BucketTag is not null)
            {
                item.AddTagInstance(contextItem.BucketTag, true);
            }

            if (item.PriorityNumber is null && contextItem.Priority is not null)
            {
                item.AddTagInstance(contextItem.Priority, true);
            }

            if (item.RepeatName is null && contextItem.RepeatTag is not null)
            {
                item.AddTagInstance(contextItem.RepeatTag, true);

                var ti = contextItem.GetTagInstance(enTagType.Repeating);

                if (ti is not null)
                {
                    item.AddTagInstance(ti, true);
                }
            }

            foreach (var contextCustomTag in contextItem.TagInstances.Where(_ => _.Type == enTagType.Custom))
            {
                if (!item.TagInstances.Any(_ => _.Type == contextCustomTag.Type && _.DefinitionName == contextCustomTag.DefinitionName))
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
