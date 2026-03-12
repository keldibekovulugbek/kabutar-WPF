using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kabutar_WPF.Models.Chat
{
    public class ChatItem : INotifyPropertyChanged
    {
        private bool _isOnline;
        private DateTime? _lastActive;
        private string _name = string.Empty;
        private string _lastMessage = string.Empty;
        private DateTime _lastMessageTime;
        private int _unreadCount;

        public long Id { get; set; }
        public string? ProfilePicture { get; set; }
        public string? ProfilePictureThumbnail { get; set; }
        public string? ThumbnailOrProfile => ProfilePictureThumbnail ?? ProfilePicture;
        public bool IsGroup { get; set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string LastMessage
        {
            get => _lastMessage;
            set { _lastMessage = value; OnPropertyChanged(); }
        }

        public DateTime LastMessageTime
        {
            get => _lastMessageTime;
            set { _lastMessageTime = value; OnPropertyChanged(); }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set { _unreadCount = value; OnPropertyChanged(); }
        }

        public bool IsOnline
        {
            get => _isOnline;
            set { _isOnline = value; OnPropertyChanged(); }
        }

        public DateTime? LastActive
        {
            get => _lastActive;
            set { _lastActive = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
