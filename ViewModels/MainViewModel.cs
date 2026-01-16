using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Kabutar_WPF.Core;
using Kabutar_WPF.Models.Chat;
using Kabutar_WPF.Models.Messages;
using Kabutar_WPF.Models.Search;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ISearchService _searchService;
        private readonly IMessageService _messageService;
        private readonly IChatService _chatService;
        private ChatItem? _selectedChat;
        private string _searchText = string.Empty;
        private string _currentUserName = "User";
        private string _messageText = string.Empty;
        private bool _isSearching;
        private bool _isLoading;

        public MainViewModel(IAuthService authService, ISearchService searchService, IMessageService messageService, IChatService chatService)
        {
            _authService = authService;
            _searchService = searchService;
            _messageService = messageService;
            _chatService = chatService;

            Chats = new ObservableCollection<ChatItem>();
            Messages = new ObservableCollection<Message>();
            SearchResults = new ObservableCollection<object>();

            LogoutCommand = new RelayCommand(_ => Logout());
            SearchCommand = new RelayCommand(async _ => await PerformSearchAsync(), _ => !string.IsNullOrWhiteSpace(SearchText));
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => CanSendMessage());
            SelectUserCommand = new RelayCommand<UserSearchResult>(user => SelectUserFromSearch(user));
            SelectMessageResultCommand = new RelayCommand<MessageSearchResult>(msgResult => SelectMessageResult(msgResult));

            _ = LoadChatsAsync();
        }

        public ObservableCollection<ChatItem> Chats { get; }
        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<object> SearchResults { get; }

        public ChatItem? SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (SetProperty(ref _selectedChat, value))
                {
                    LoadMessages();
                    (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    IsSearching = !string.IsNullOrWhiteSpace(value);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        SearchResults.Clear();
                    }
                    else
                    {
                        _ = PerformSearchAsync();
                    }
                }
            }
        }

        public string MessageText
        {
            get => _messageText;
            set
            {
                if (SetProperty(ref _messageText, value))
                {
                    (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LogoutCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand SelectUserCommand { get; }
        public ICommand SelectMessageResultCommand { get; }

        private async Task LoadChatsAsync()
        {
            try
            {
                IsLoading = true;
                var chats = await _chatService.GetRecentChatsAsync();

                Chats.Clear();
                foreach (var chat in chats)
                {
                    Chats.Add(chat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chatlarni yuklashda xatolik: {ex.Message}", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
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

        private async Task PerformSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults.Clear();
                return;
            }

            try
            {
                IsLoading = true;
                SearchResults.Clear();

                var result = await _searchService.SearchAsync(SearchText.Trim());

                if (result != null)
                {
                    // Add users first
                    foreach (var user in result.Users)
                    {
                        SearchResults.Add(user);
                    }

                    // Then add message results
                    foreach (var message in result.Messages)
                    {
                        SearchResults.Add(message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Qidiruv xatoligi: {ex.Message}", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SelectUserFromSearch(UserSearchResult? user)
        {
            if (user == null) return;

            // Check if chat already exists
            var existingChat = Chats.FirstOrDefault(c => c.Id == user.Id);
            if (existingChat != null)
            {
                SelectedChat = existingChat;
            }
            else
            {
                // Create new chat
                var newChat = new ChatItem
                {
                    Id = user.Id,
                    Name = user.FullName,
                    ProfilePicture = user.ProfilePicture,
                    IsOnline = user.IsOnline,
                    LastMessage = "",
                    LastMessageTime = DateTime.Now,
                    UnreadCount = 0,
                    IsGroup = false
                };

                Chats.Insert(0, newChat);
                SelectedChat = newChat;
            }

            // Clear search
            SearchText = string.Empty;
            SearchResults.Clear();
            IsSearching = false;
        }

        private void SelectMessageResult(MessageSearchResult? msgResult)
        {
            if (msgResult == null) return;

            // Find or create chat for this user
            var existingChat = Chats.FirstOrDefault(c => c.Id == msgResult.UserId);
            if (existingChat != null)
            {
                SelectedChat = existingChat;
            }
            else
            {
                var newChat = new ChatItem
                {
                    Id = msgResult.UserId,
                    Name = msgResult.FullName,
                    ProfilePicture = msgResult.ProfilePicture,
                    LastMessage = msgResult.MessageContent,
                    LastMessageTime = msgResult.SentAt,
                    UnreadCount = 0,
                    IsGroup = false
                };

                Chats.Insert(0, newChat);
                SelectedChat = newChat;
            }

            // Clear search
            SearchText = string.Empty;
            SearchResults.Clear();
            IsSearching = false;
        }

        private bool CanSendMessage()
        {
            return SelectedChat != null &&
                   !string.IsNullOrWhiteSpace(MessageText) &&
                   !IsLoading;
        }

        private async Task SendMessageAsync()
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(MessageText))
                return;

            try
            {
                IsLoading = true;

                var request = new SendMessageRequest
                {
                    ReceiverId = SelectedChat.Id,
                    Content = MessageText.Trim()
                };

                var success = await _messageService.SendMessageAsync(request);

                if (success)
                {
                    // Add message to UI
                    var newMessage = new Message
                    {
                        Id = DateTime.Now.Ticks, // Temporary ID
                        ChatId = SelectedChat.Id,
                        Content = MessageText.Trim(),
                        SentAt = DateTime.Now,
                        IsFromMe = true,
                        IsRead = false,
                        IsSent = true
                    };

                    Messages.Add(newMessage);

                    // Update last message in chat list
                    SelectedChat.LastMessage = MessageText.Trim();
                    SelectedChat.LastMessageTime = DateTime.Now;

                    // Clear input
                    MessageText = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xabar yuborishda xatolik: {ex.Message}", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Logout()
        {
            _authService.ClearToken();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var loginView = new Views.Auth.LoginView();
                loginView.Show();

                // Close current window
                foreach (Window window in Application.Current.Windows)
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
