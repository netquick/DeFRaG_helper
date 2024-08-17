using DeFRaG_Helper.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace DeFRaG_Helper.ViewModels
{
    public class TagBarViewModel : INotifyPropertyChanged
    {
        private static TagBarViewModel _instance;
        private ObservableCollection<TagItem> tags;

        // Singleton instance
        public static TagBarViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TagBarViewModel();
                }
                return _instance;
            }
        }

        public ObservableCollection<TagItem> Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        // Private constructor to prevent direct instantiation
        private TagBarViewModel()
        {
            Tags = new ObservableCollection<TagItem>();

            // Load tags from the "Tag" table first
            LoadTagsFromDatabase();

            // Add all possible weapons, items, and functions to tags
            foreach (var weapon in TagOptions.Weapons)
            {
                Tags.Add(new TagItem { Name = weapon.Key, IconPath = weapon.Value.path, Color = weapon.Value.color });
            }

            foreach (var item in TagOptions.Items)
            {
                Tags.Add(new TagItem { Name = item.Key, IconPath = item.Value.path, Color = item.Value.color });
            }

            foreach (var function in TagOptions.Functions)
            {
                Tags.Add(new TagItem { Name = function.Key, IconPath = function.Value.path, Color = function.Value.color });
            }
        }

        private async void LoadTagsFromDatabase()
        {
            var customTags = new List<TagItem>();

            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand("SELECT Tag FROM Tag", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string tag = reader.GetString(0);
                            customTags.Add(new TagItem { Name = tag });
                        }
                    }
                }
            });
            await DbQueue.Instance.WhenAllCompleted();

            // Insert custom tags at the beginning of the Tags collection
            foreach (var tag in customTags)
            {
                Tags.Insert(0, tag);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddTag(TagItem tagItem)
        {
            if (!Tags.Any(t => t.Name == tagItem.Name))
            {
                Tags.Insert(0, tagItem); // Insert at the beginning
            }
        }

        public void RemoveTag(string tagName)
        {
            var tagItem = Tags.FirstOrDefault(t => t.Name == tagName);
            if (tagItem != null)
            {
                Tags.Remove(tagItem);
            }
        }
    }

    public class TagItem
    {
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string Color { get; set; }
    }
}
