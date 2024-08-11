using DeFRaG_Helper.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DeFRaG_Helper.ViewModels
{
    public class TagBarViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TagItem> tags;

        public ObservableCollection<TagItem> Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        public TagBarViewModel()
        {
            Tags = new ObservableCollection<TagItem>();

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TagItem
    {
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string Color { get; set; }
    }

}
