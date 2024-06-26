﻿using Jiminy.Helpers;
using System.Collections.Generic;
using System.Linq;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    /// <summary>
    /// Stores and manages the set of jiminy items that have been gathered from all the monitored files
    /// </summary>
    internal class ItemRegistry
    {
        static readonly object _lock = new();

        private List<Item> _items = new();

        public ItemRegistry()
        {

        }

        public ItemRegistry(ItemSubSet items)
        {
            _items.AddRange(items.Items);
        }

        public List<Item> Items => _items;

        // Gets populated on request just before building outputs
        public List<Item> Duplicates = new();

        // Some items get read an awful lot when generating the
        // report, so we cache filtered lists for them

        private ItemSubSet? _reminderItems = new();

        internal ItemSubSet DatedItems
        {
            get
            {
                if (_reminderItems is null)
                {
                    _reminderItems = new(OpenItems.Items.Where(_ => _.ReminderDateTime != null || _.DueDateTime != null));
                }

                return _reminderItems;
            }
        }

        private ItemSubSet? _openItems = new();

        internal ItemSubSet OpenItems
        {
            get
            {
                if (_openItems is null)
                {
                    _openItems = new(_items.Where(_ => _.IsCompleted == false && _.SetsContext == false && _.ClearsContext == false).OrderBy(_ => _.PriorityNumber));
                }

                return _openItems;
            }
        }

        private ItemSubSet? _projectItems = null;

        internal ItemSubSet ProjectItems
        {
            get
            {
                if (_projectItems is null)
                {
                    _projectItems = new(OpenItems.Items.Where(_ => _.ProjectName.NotEmpty()).OrderBy(_ => _.ProjectName).ThenBy(_ => _.PriorityNumber));
                }

                return _projectItems ?? new ItemSubSet();
            }
        }

        internal ItemSubSet CompletedItems => new(_items.Where(_ => _.IsCompleted == true && _.SetsContext == false && _.ClearsContext == false).OrderBy(_ => _.PriorityNumber));
        internal ItemSubSet BucketItems => new(OpenItems.Items.Where(_ => _.BucketName is not null));

        internal ItemSubSet OverdueItems => new(DatedItems.Items.Where(_ => _.MostUrgentDateStatus == enDateStatus.Overdue));
        internal ItemSubSet TodayItems => new(DatedItems.Items.Where(_ => _.MostUrgentDateStatus == enDateStatus.Today));
        internal ItemSubSet SoonItems => new(DatedItems.Items.Where(_ => _.MostUrgentDateStatus == enDateStatus.Soon));
        internal ItemSubSet FutureItems => new(DatedItems.Items.Where(_ => _.MostUrgentDateStatus == enDateStatus.Future));

        internal List<string> SourceFileNames => _items.Select(_ => _.SourceFileName!).Distinct().ToList();

        internal void CheckForDuplicates()
        {
            foreach (var it in _items)
            {
                it.GenerateHash();
            }

            Duplicates = new();

            var duplicateHashes = _items
                .Where(_ => _.ClearsContext == false & _.SetsContext == false)
                .GroupBy(_ => _.Hash)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key).ToList();

            foreach (var dupHash in duplicateHashes)
            {
                Duplicates.AddRange(_items.Where(_ => _.Hash == dupHash));
            }
        }

        /// <summary>
        /// Gneerates the set of items for a specific output file according to the 
        /// filters specified for that output.
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        internal ItemRegistry GenerateRegistryForOutputFile(OutputSpecification spec)
        {
            // If there are no filters, just return the whole registry, otherwise
            // build up a subset of this one

            if (spec.ItemSelection is not null)
            {
                IEnumerable<Item>? items = null;

                if (spec.ItemSelection.MustMatchAll)
                {
                    items = _items
                        .Where(it =>
                            (
                                !spec.ItemSelection.IncludeProjectNames.Any()
                                ||
                                spec.ItemSelection.IncludeProjectNames.Contains(it.ProjectName!)
                            )
                            &&
                            (
                                !spec.ItemSelection.IncludeTagNames.Any()
                                ||
                                spec.ItemSelection.IncludeTagNames.Any(tn => it.TagInstances.Select(ti => ti.Type.ToString()).Contains(tn))
                            ));
                }
                else
                {
                    items = _items
                        .Where(it =>
                            spec.ItemSelection.IncludeProjectNames.Contains(it.ProjectName!)
                            ||
                            spec.ItemSelection.IncludeTagNames.Any(tn => it.TagInstances.Select(ti => ti.DefinitionName).Contains(tn)));
                }

                ItemRegistry newReg = new(new ItemSubSet(items));

                newReg.RefreshCaches();

                return newReg;
            }
            else
            {
                return this;
            }
        }

        internal bool HasItemsFromFilePath(string filePath)
        {
            return _items.Any(_ => _.SourceFileName == filePath);
        }

        internal void Add(Item item, string? replaceFromFileName = null)
        {
            AddRange(new List<Item> { item }, replaceFromFileName);
        }

        internal void AddRange(List<Item> items, string? replaceFromFileName = null)
        {
            lock (_lock)
            {
                if (replaceFromFileName is not null)
                {
                    if (HasItemsFromFilePath(replaceFromFileName))
                    {
                        // Going backwards through the collection means we can prune it on the fly

                        for (int i = _items.Count - 1; i >= 0; i--)
                        {
                            if (_items[i].SourceFileName == replaceFromFileName)
                            {
                                _items.RemoveAt(i);
                            }
                        }
                    }
                }

                _items.AddRange(items);

                RefreshCaches();
            }
        }

        private void RefreshCaches()
        {
            // Force regeneration of cached lists

            _openItems = null;
            _projectItems = null;
            _reminderItems = null;
        }

        /// <summary>
        /// Used when a source file is deleted, clears out any items that came from that file
        /// </summary>
        /// <param name="fullPath"></param>
        internal void DeleteItemsFoundInFile(string fullPath)
        {
            _items.RemoveAll(_ => _.SourceFileName == fullPath);
        }
    }
}

