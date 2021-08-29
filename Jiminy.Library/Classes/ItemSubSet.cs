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

        public ItemSubSet Filter(string? onlyProjectName = null, enPriority? onlyPriority = null, string? onlyBucketName = null, enRepeat? onlyRepeat = null)
        {
            if (onlyProjectName is null && onlyPriority is null && onlyBucketName is null && onlyRepeat is null)
            {
                return this;
            }
            else
            {
                var filtered = _items.Where(_ =>
                    (onlyProjectName == null || _.ProjectName == onlyProjectName) &&
                    (onlyPriority == null || _.Priority == onlyPriority) &&
                    (onlyBucketName == null || _.BucketName == onlyBucketName) &&
                    (onlyRepeat == null || _.Repeat == onlyRepeat));

                return new ItemSubSet(filtered);
            }
        }
    }
}
