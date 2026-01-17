using System.Windows;
using System.Windows.Controls;
using Kabutar_WPF.Models.Chat;

namespace Kabutar_WPF.Helpers
{
    public class MessageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DateSeparatorTemplate { get; set; }
        public DataTemplate? MessageTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is MessageItem messageItem)
            {
                return messageItem.IsDateSeparator ? DateSeparatorTemplate : MessageTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
