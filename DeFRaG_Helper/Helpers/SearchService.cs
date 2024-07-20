namespace DeFRaG_Helper
{
    public class SearchService
    {
        public IEnumerable<ISearchableItem> Search(IEnumerable<ISearchableItem> items, string query)
        {
            // Simple case-insensitive search implementation
            return items.Where(item => item.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }

}
