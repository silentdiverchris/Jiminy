using Jiminy.Helpers;
using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    internal class ItemRegistry
    {
        static readonly object _lock = new();

        private ProjectRegistry _projectRegistry = new();

        private List<Item> _items = new();

        internal List<Item> Items => _items;

        // Some items get read an awful lot when generating the
        // report, so we cache filtered lists for them

        private ItemSubSet? _reminderItems = new();

        internal ItemSubSet ReminderItems
        {
            get
            {
                if (_reminderItems is null)
                {
                    _reminderItems = new(OpenItems.Items.Where(_ => _.ReminderDateTime != null).OrderBy(_ => _.ReminderDateTime).ThenBy(_ => _.Priority));
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
                    _openItems = new(_items.Where(_ => _.IsCompleted == false && _.IsContext == false).OrderBy(_ => _.Priority));
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
                    _projectItems = new(OpenItems.Items.Where(_ => !string.IsNullOrEmpty(_.ProjectName)).OrderBy(_ => _.ProjectName).ThenBy(_ => _.Priority));
                }

                return _projectItems ?? new ItemSubSet();
            }
        }

        internal ItemSubSet CompletedItems => new(_items.Where(_ => _.IsCompleted == true && _.IsContext == false).OrderBy(_ => _.Priority));
        internal ItemSubSet BucketItems => new(OpenItems.Items.Where(_ => _.BucketName is not null));

        internal ProjectRegistry ProjectRegistry => _projectRegistry;
        internal List<string> ProjectNames => _projectRegistry.Projects.Select(_ => _.Name).ToList();

        //internal List<string> FoundInFiles => _items.Select(_ => _.FullFileName).Distinct().OrderBy(_ => _).Select(_ => new Item { FullFileName = _ });

        internal bool HasItemsFromFilePath(string filePath)
        {
            return _items.Any(_ => _.FullFileName == filePath);
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
                            if (_items[i].FullFileName == replaceFromFileName)
                            {
                                _items.RemoveAt(i);
                            }
                        }
                    }
                }

                _items.AddRange(items);

                var projectNames = _items.Where(_ => !string.IsNullOrEmpty(_.ProjectName)).Select(_ => _.ProjectName).Distinct().ToList();

                _projectRegistry = new(projectNames!);

                // Force regeneration of cached lists

                _openItems = null;
                _projectItems = null;
                _reminderItems = null;
            }
        }
    }
}

