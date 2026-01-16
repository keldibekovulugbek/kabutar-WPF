using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Kabutar_WPF.Core;
using Kabutar_WPF.Models.Chat;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private ChatItem? _selectedChat;
        private string _searchText = string.Empty;
        private string _currentUserName = "User";

        public MainViewModel(IAuthService authService)
        {
            _authService = authService;

            Chats = new ObservableCollection<ChatItem>();
            Messages = new ObservableCollection<Message>();

            LogoutCommand = new RelayCommand(_ => Logout());
            SearchCommand = new RelayCommand(_ => SearchChats());

            LoadChats();
        }

        public ObservableCollection<ChatItem> Chats { get; }
        public ObservableCollection<Message> Messages { get; }

        public ChatItem? SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (SetProperty(ref _selectedChat, value))
                {
                    LoadMessages();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public ICommand LogoutCommand { get; }
        public ICommand SearchCommand { get; }

        private void LoadChats()
        {
            // TODO: Load chats from API
            // For now, add some dummy data
            Chats.Add(new ChatItem
            {
                Id = 1,
                Name = "Ulugbek Keldibekov",
                LastMessage = "Salom, qalaysiz?",
                LastMessageTime = DateTime.Now.AddMinutes(-5),
                UnreadCount = 2,
                IsOnline = true,
                IsGroup = false
            });

            Chats.Add(new ChatItem
            {
                Id = 2,
                Name = "Developers Group",
                LastMessage = "Yangi task qo'shildi",
                LastMessageTime = DateTime.Now.AddHours(-1),
                UnreadCount = 5,
                IsOnline = false,
                IsGroup = true
            });
        }

        private void LoadMessages()
        {
            Messages.Clear();

            if (SelectedChat == null) return;

            // TODO: Load messages from API
            // For now, add some dummy messages
            Messages.Add(new Message
            {
                Id = 1,
                ChatId = SelectedChat.Id,
                Content = "Salom!",
                SentAt = DateTime.Now.AddMinutes(-10),
                IsFromMe = false,
                IsRead = true,
                IsSent = true
            });

            Messages.Add(new Message
            {
                Id = 2,
                ChatId = SelectedChat.Id,
                Content = "Qalaysiz?",
                SentAt = DateTime.Now.AddMinutes(-5),
                IsFromMe = true,
                IsRead = true,
                IsSent = true
            });
        }

        private void SearchChats()
        {
            // TODO: Implement chat search
        }

        private void Logout()
        {
            _authService.ClearToken();

            // TODO: Navigate back to login
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var loginView = new Views.Auth.LoginView();
                loginView.Show();

                // Close current window
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.Close();
                        break;
                    }
                }
            });
        }
    }
}
