using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
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

        public List<Item> Items => _items;

        public ItemSubSet Filter(
            ProjectDefinition? onlyProject = null, 
            string? onlyPriorityName = null, 
            string? onlyBucketName = null, 
            string? onlyRepeatName = null)
        {
            if (onlyProject is null && onlyPriorityName is null && onlyBucketName is null && onlyRepeatName is null)
            {
                return this;
            }
            else
            {
                var filtered = _items.Where(_ =>
                    (onlyProject == null || _.ProjectName == onlyProject.Name) &&
                    (onlyPriorityName == null || _.PriorityName == onlyPriorityName) &&
                    (onlyBucketName == null || _.BucketName == onlyBucketName) &&
                    (onlyRepeatName == null || _.RepeatName == onlyRepeatName));

                return new ItemSubSet(filtered);
            }
        }
    }
}
