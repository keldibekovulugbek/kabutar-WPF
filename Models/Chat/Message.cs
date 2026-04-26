using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kabutar_WPF.Models.Chat
{
    public class Message : INotifyPropertyChanged
    {
        private bool _isRead;
        private bool _isEditing;
        private string _editText = string.Empty;
        private bool _isEdited;

        public long Id { get; set; }
        public long ChatId { get; set; }
        public long SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }

        public bool IsRead
        {
            get => _isRead;
            set { _isRead = value; OnPropertyChanged(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditing)); }
        }

        public bool IsNotEditing => !_isEditing;

        public string EditText
        {
            get => _editText;
            set { _editText = value; OnPropertyChanged(); }
        }

        public bool IsEdited
        {
            get => _isEdited;
            set { _isEdited = value; OnPropertyChanged(); }
        }

        public bool IsSent { get; set; }
        public bool IsFromMe { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool HasImage => !string.IsNullOrEmpty(AttachmentUrl);

        // Reply
        public long? ReplyToMessageId { get; set; }
        public string? ReplyToContent { get; set; }
        public string? ReplyToSenderName { get; set; }
        public bool HasReply => ReplyToMessageId.HasValue;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
