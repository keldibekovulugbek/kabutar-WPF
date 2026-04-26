using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kabutar_WPF.Models.Chat
{
    public class MessageItem : INotifyPropertyChanged
    {
        public bool IsDateSeparator { get; set; }
        public string? DateText { get; set; }
        public Message? Message { get; set; }

        public void NotifyMessageChanged()
        {
            OnPropertyChanged(nameof(Message));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
