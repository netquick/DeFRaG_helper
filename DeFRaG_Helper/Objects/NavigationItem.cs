using System.ComponentModel;

namespace DeFRaG_Helper
{
    public class NavigationItem : INotifyPropertyChanged
    {
        private bool _isActive;
        public string Icon { get; set; }
        public string Text { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
