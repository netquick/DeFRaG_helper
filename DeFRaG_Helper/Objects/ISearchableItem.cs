namespace DeFRaG_Helper
{
    public interface ISearchableItem
    {
        string DisplayName { get; }
        string NavigationPath { get; }
        string MapIdentifier { get; }

    }

}
