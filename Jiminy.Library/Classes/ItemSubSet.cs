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

        public ItemSubSet Filter(string? onlyProjectName = null, enPriority? onlyPriority = null, enBucket? onlyBucket = null, bool onlyDaily = false, bool onlyWeekly = false, bool onlyMonthly = false)
        {
            if (onlyProjectName is null && onlyPriority is null && onlyBucket is null && !onlyDaily && !onlyWeekly && !onlyMonthly)
            {
                return this;
            }
            else
            {
                var filtered = _items.Where(_ =>
                    (onlyProjectName == null || _.ProjectName == onlyProjectName) &&
                    (onlyPriority == null || _.Priority == onlyPriority) &&
                    (onlyBucket == null || _.Bucket == onlyBucket) &&
                    (!onlyDaily || _.IsDaily) &&
                    (!onlyWeekly || _.IsWeekly) &&
                    (!onlyMonthly || _.IsMonthly));

                return new ItemSubSet(filtered);
            }
        }
    }
}
