using System.Windows;
using System.Windows.Controls;
using DeFRaG_Helper.ViewModels;

namespace DeFRaG_Helper.Helpers
{
    public class TagTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IconTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var tagItem = item as TagItem;
            if (tagItem != null && !string.IsNullOrEmpty(tagItem.IconPath))
            {
                return IconTemplate;
            }
            return TextTemplate;
        }
    }
}
