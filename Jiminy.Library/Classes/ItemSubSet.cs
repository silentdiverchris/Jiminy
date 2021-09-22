using System.Collections.Generic;
using System.Linq;

namespace Jiminy.Classes
{
    /// <summary>
    /// Represents a subset of items, usually as the result of filtering the main set
    /// </summary>
    internal class ItemSubSet
    {
        private List<Item> _items = new();

        public ItemSubSet(IEnumerable<Item>? items = null)
        {
            _items = items is not null
                ? items.ToList()
                : new List<Item>();
        }

        public bool Any => _items.Any();
        public int Count => _items.Count;

        public List<Item> Items => _items;

        public ItemSubSet Filter(
            ProjectDefinition? onlyProject = null, 
            string? onlyPriorityName = null, 
            string? onlyBucketName = null, 
            string? onlyRepeatName = null,
            string? onlyWithTagName = null)
        {
            if (onlyProject is null && onlyPriorityName is null && onlyBucketName is null && onlyRepeatName is null && onlyWithTagName is null)
            {
                return this;
            }
            else
            {
                var filtered = _items.Where(_ =>
                    (onlyWithTagName == null || _.TagInstances.Any(ti => ti.DefinitionName == onlyWithTagName)) &&
                    (onlyProject == null || _.ProjectName == onlyProject.Name) &&
                    (onlyPriorityName == null || _.PriorityName == onlyPriorityName) &&
                    (onlyBucketName == null || _.BucketName == onlyBucketName) &&
                    (onlyRepeatName == null || _.RepeatName == onlyRepeatName));

                return new ItemSubSet(filtered);
            }
        }
    }
}
