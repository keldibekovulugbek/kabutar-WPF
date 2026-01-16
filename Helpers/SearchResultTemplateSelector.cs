using System.Windows;
using System.Windows.Controls;
using Kabutar_WPF.Models.Search;

namespace Kabutar_WPF.Views
{
    public class SearchResultTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UserTemplate { get; set; }
        public DataTemplate? MessageTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is UserSearchResult)
            {
                return UserTemplate;
            }
            else if (item is MessageSearchResult)
            {
                return MessageTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
