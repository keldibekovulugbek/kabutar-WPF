using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kabutar_WPF.Core;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.Models.Chat;
using Kabutar_WPF.Models.Messages;
using Kabutar_WPF.Models.Search;
using Kabutar_WPF.Models.Users;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ISearchService _searchService;
        private readonly IMessageService _messageService;
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IWindowNavigationService _navigation;

        private ChatItem? _selectedChat;
        private string _searchText = string.Empty;
        private string _messageText = string.Empty;
        private bool _isSearching;
        private bool _isLoading;
        private bool _isTyping;
        private System.Windows.Threading.DispatcherTimer? _typingStopTimer;
        private bool _isRemoteTyping;
        private System.Windows.Threading.DispatcherTimer? _remoteTypingTimer;
        private EventHandler? _remoteTypingTickHandler;
        private long _myUserId;
        private CancellationTokenSource? _loadMessagesCts;
        private long? _lastLoadedChatId;
        private SignalRService? _signalRService;
        private MessageItem? _replyingTo;

        // Lazy loading
        private int _currentPage = 1;
        private bool _hasMoreMessages;
        private bool _isLoadingMore;

        // Cache
        private readonly System.Collections.Generic.Dictionary<long, (List<Services.MessageDTO> messages, DateTime loadedAt)> _messageCache = new();
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        private string _menuUserName = string.Empty;
        private string _menuUserInitials = string.Empty;
        private BitmapSource? _menuUserProfileImage;
        private ImageBrush? _chatBackgroundBrush;

        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public event Action? RequestClose;

        public MainViewModel(
            IAuthService authService,
            ISearchService searchService,
            IMessageService messageService,
            IChatService chatService,
            IUserService userService,
            IWindowNavigationService navigation)
        {
            _authService = authService;
            _searchService = searchService;
            _messageService = messageService;
            _chatService = chatService;
            _userService = userService;
            _navigation = navigation;
            _myUserId = _authService.GetUserId() ?? 0;

            Chats = new ObservableCollection<ChatItem>();
            Messages = new ObservableCollection<MessageItem>();
            SearchResults = new ObservableCollection<object>();

            LogoutCommand = new RelayCommand(_ => Logout());
            SearchCommand = new RelayCommand(async _ => await PerformSearchAsync(), _ => !string.IsNullOrWhiteSpace(SearchText));
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => CanSendMessage());
            SelectUserCommand = new RelayCommand<UserSearchResult>(user => SelectUserFromSearch(user));
            SelectMessageResultCommand = new RelayCommand<MessageSearchResult>(msgResult => SelectMessageResult(msgResult));
            DeleteMessageCommand = new RelayCommand<MessageItem>(async item => await DeleteMessageAsync(item, false));
            DeleteMessageForBothCommand = new RelayCommand<MessageItem>(async item => await DeleteMessageAsync(item, true));
            ClearChatCommand = new RelayCommand<ChatItem>(item => ShowClearChatDialog(item));
            ClearChatForBothCommand = new RelayCommand<ChatItem>(item => ShowClearChatDialog(item));
            SendImageCommand = new RelayCommand<string>(async path => await SendImageAsync(path));
            EditMessageCommand = new RelayCommand<MessageItem>(item => BeginEditMessage(item));
            ConfirmEditCommand = new RelayCommand<MessageItem>(async item => await ConfirmEditAsync(item));
            CancelEditCommand = new RelayCommand<MessageItem>(item => CancelEdit(item));
            LoadMoreMessagesCommand = new RelayCommand(async _ => await LoadMoreMessagesAsync(), _ => HasMoreMessages && !IsLoadingMore);
            ReplyCommand = new RelayCommand<MessageItem>(item => { ReplyingTo = item; });
            CancelReplyCommand = new RelayCommand(_ => ReplyingTo = null);

            _ = LoadChatsAsync();
        }

        public ObservableCollection<ChatItem> Chats { get; }
        public ObservableCollection<MessageItem> Messages { get; }
        public ObservableCollection<object> SearchResults { get; }

        public ChatItem? SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (SetProperty(ref _selectedChat, value))
                {
                    _ = LoadMessagesAsync();
                    (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public void NotifySelectedChatChanged() => OnPropertyChanged(nameof(SelectedChat));

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    IsSearching = !string.IsNullOrWhiteSpace(value);
                    if (string.IsNullOrWhiteSpace(value))
                        SearchResults.Clear();
                    else
                        _ = PerformSearchAsync();
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
                    HandleTypingIndicator();
                }
            }
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

        public bool IsRemoteTyping
        {
            get => _isRemoteTyping;
            set => SetProperty(ref _isRemoteTyping, value);
        }

        public string MenuUserName
        {
            get => _menuUserName;
            set => SetProperty(ref _menuUserName, value);
        }

        public string MenuUserInitials
        {
            get => _menuUserInitials;
            set => SetProperty(ref _menuUserInitials, value);
        }

        public BitmapSource? MenuUserProfileImage
        {
            get => _menuUserProfileImage;
            set
            {
                SetProperty(ref _menuUserProfileImage, value);
                OnPropertyChanged(nameof(HasProfileImage));
            }
        }

        public bool HasProfileImage => _menuUserProfileImage != null;

        public ImageBrush? ChatBackgroundBrush
        {
            get => _chatBackgroundBrush;
            set
            {
                SetProperty(ref _chatBackgroundBrush, value);
                OnPropertyChanged(nameof(HasChatBackground));
            }
        }

        public bool HasChatBackground => _chatBackgroundBrush != null;

        public bool HasMoreMessages
        {
            get => _hasMoreMessages;
            set { SetProperty(ref _hasMoreMessages, value); (LoadMoreMessagesCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            set { SetProperty(ref _isLoadingMore, value); (LoadMoreMessagesCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public ICommand LogoutCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand SelectUserCommand { get; }
        public ICommand SelectMessageResultCommand { get; }
        public ICommand DeleteMessageCommand { get; }
        public ICommand DeleteMessageForBothCommand { get; }
        public ICommand ClearChatCommand { get; }
        public ICommand ClearChatForBothCommand { get; }
        public ICommand SendImageCommand { get; }
        public MessageItem? ReplyingTo
        {
            get => _replyingTo;
            set
            {
                SetProperty(ref _replyingTo, value);
                OnPropertyChanged(nameof(IsReplying));
                OnPropertyChanged(nameof(ReplyingToSenderName));
                OnPropertyChanged(nameof(ReplyingToPreview));
            }
        }

        public bool IsReplying => _replyingTo != null;

        public string ReplyingToSenderName =>
            _replyingTo?.Message?.IsFromMe == true ? "Siz" : SelectedChat?.Name ?? "";

        public string ReplyingToPreview =>
            _replyingTo?.Message?.HasImage == true
                ? "📷 Rasm"
                : (_replyingTo?.Message?.Content ?? "");

        public ICommand EditMessageCommand { get; }
        public ICommand ConfirmEditCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand LoadMoreMessagesCommand { get; }
        public ICommand ReplyCommand { get; }
        public ICommand CancelReplyCommand { get; }

        public Func<ChatItem, (bool confirmed, bool deleteForBoth)>? ShowClearChatDialogFunc { get; set; }

        public async Task InitializeAsync()
        {
            await LoadUserProfileAsync();
            await LoadUserSettingsAsync();
        }

        private async Task LoadUserProfileAsync()
        {
            try
            {
                var profile = await _userService.GetCurrentUserAsync();
                if (profile == null) return;

                var fullName = profile.Fullname;
                if (string.IsNullOrWhiteSpace(fullName))
                    fullName = $"{profile.FirstName} {profile.LastName}".Trim();

                MenuUserName = string.IsNullOrWhiteSpace(fullName) ? profile.Username ?? string.Empty : fullName;

                var fi = string.IsNullOrEmpty(profile.FirstName) ? "" : profile.FirstName[0].ToString();
                var li = string.IsNullOrEmpty(profile.LastName) ? "" : profile.LastName[0].ToString();
                var initials = (fi + li).ToUpper();
                if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(profile.Username))
                    initials = profile.Username[0].ToString().ToUpper();
                MenuUserInitials = initials;

                if (!string.IsNullOrEmpty(profile.ProfilePicture) && !profile.ProfilePicture.Contains("default"))
                    MenuUserProfileImage = await LoadBitmapAsync(GetFullImageUrl(profile.ProfilePicture));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
            }
        }

        public async Task LoadUserSettingsAsync()
        {
            try
            {
                var settings = await _userService.GetSettingsAsync();
                if (settings == null) return;

                ApplyTheme(settings.Theme);
                ApplyFontSize(settings.FontSize);

                if (!string.IsNullOrEmpty(settings.ChatBackgroundImage))
                {
                    var bitmap = await LoadBitmapAsync(GetFullImageUrl(settings.ChatBackgroundImage));
                    if (bitmap != null)
                    {
                        ChatBackgroundBrush = new ImageBrush(bitmap)
                        {
                            Stretch = Stretch.UniformToFill,
                            Opacity = 0.3
                        };
                    }
                }
                else
                {
                    ChatBackgroundBrush = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user settings: {ex.Message}");
            }
        }

        public static void ApplyTheme(string theme)
        {
            try
            {
                var mergedDicts = Application.Current.Resources.MergedDictionaries;
                ResourceDictionary? toRemove = null;
                foreach (var dict in mergedDicts)
                {
                    if (dict.Source != null &&
                        (dict.Source.OriginalString.Contains("LightTheme") ||
                         dict.Source.OriginalString.Contains("DarkTheme")))
                    {
                        toRemove = dict;
                        break;
                    }
                }
                if (toRemove != null)
                    mergedDicts.Remove(toRemove);

                var uri = theme == "dark"
                    ? new Uri("pack://application:,,,/Resources/Themes/DarkTheme.xaml")
                    : new Uri("pack://application:,,,/Resources/Themes/LightTheme.xaml");
                mergedDicts.Add(new ResourceDictionary { Source = uri });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }

        public static void ApplyFontSize(string fontSize)
        {
            double value = fontSize switch
            {
                "small" => 12.0,
                "large" => 18.0,
                _ => 14.0
            };
            Application.Current.Resources["ChatFontSize"] = value;
        }

        private async Task SendImageAsync(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || SelectedChat == null) return;
            try
            {
                var messageService = new MessageService(ApiClient.Instance);
                var success = await messageService.SendImageAsync(SelectedChat.Id, filePath);
                if (success)
                {
                    var now = DateTime.Now;
                    var newMessage = new Message
                    {
                        Id = now.Ticks,
                        ChatId = SelectedChat.Id,
                        Content = "📷 Rasm",
                        AttachmentUrl = filePath,
                        SentAt = now,
                        IsFromMe = true,
                        IsSent = true
                    };
                    Messages.Add(new MessageItem { Message = newMessage });
                    SelectedChat.LastMessage = "📷 Rasm";
                    SelectedChat.LastMessageTime = now;
                    var index = Chats.IndexOf(SelectedChat);
                    if (index > 0) Chats.Move(index, 0);
                    NotificationService.Show("Rasm yuborildi", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Rasm yuborishda xatolik: {ex.Message}", NotificationType.Error);
            }
        }

        public async Task<UserProfileDTO?> GetChatUserProfileAsync(long userId)
        {
            try
            {
                return await ApiClient.Instance.GetAsync<UserProfileDTO>($"users/{userId}");
            }
            catch
            {
                return null;
            }
        }

        private async Task LoadChatsAsync()
        {
            try
            {
                IsLoading = true;
                var chats = await _chatService.GetRecentChatsAsync();
                Chats.Clear();
                foreach (var chat in chats)
                    Chats.Add(chat);
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Chatlarni yuklashda xatolik: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMessagesAsync()
        {
            if (SelectedChat == null) return;
            if (_lastLoadedChatId == SelectedChat.Id) return;

            _loadMessagesCts?.Cancel();
            _loadMessagesCts = new CancellationTokenSource();
            var token = _loadMessagesCts.Token;

            try
            {
                IsLoading = true;
                Messages.Clear();
                _currentPage = 1;
                HasMoreMessages = false;

                List<Services.MessageDTO> messages;
                var chatId = SelectedChat.Id;

                // Check cache
                if (_messageCache.TryGetValue(chatId, out var cached) && DateTime.Now - cached.loadedAt < CacheTtl)
                {
                    messages = cached.messages;
                }
                else
                {
                    messages = await _messageService.GetConversationAsync(chatId, 1, 50);
                    _messageCache[chatId] = (messages, DateTime.Now);
                }

                _lastLoadedChatId = chatId;
                HasMoreMessages = messages.Count >= 50;

                if (token.IsCancellationRequested) return;

                AppendMessages(messages, prepend: false);

                if (!token.IsCancellationRequested && SelectedChat != null)
                    SelectedChat.UnreadCount = 0;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    NotificationService.Show($"Xabarlarni yuklashda xatolik: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    IsLoading = false;
            }
        }

        private async Task LoadMoreMessagesAsync()
        {
            if (SelectedChat == null || !HasMoreMessages) return;
            try
            {
                IsLoadingMore = true;
                _currentPage++;
                var older = await _messageService.GetConversationAsync(SelectedChat.Id, _currentPage, 50);
                HasMoreMessages = older.Count >= 50;
                AppendMessages(older, prepend: true);
            }
            catch (Exception ex)
            {
                _currentPage--;
                NotificationService.Show($"Xabarlarni yuklashda xatolik: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        private void AppendMessages(List<Services.MessageDTO> messages, bool prepend)
        {
            if (SelectedChat == null) return;

            var items = new System.Collections.Generic.List<MessageItem>();
            DateTime? trackDate = null;
            foreach (var msg in messages)
            {
                var messageDate = msg.Created.ToLocalTime().Date;
                if (trackDate == null || trackDate.Value.Date != messageDate)
                {
                    items.Add(new MessageItem { IsDateSeparator = true, DateText = FormatDateSeparator(messageDate) });
                    trackDate = messageDate;
                }
                var isFromMe = msg.SenderId == _myUserId;
                items.Add(new MessageItem
                {
                    IsDateSeparator = false,
                    Message = new Message
                    {
                        Id = msg.Id, ChatId = SelectedChat.Id, SenderId = msg.SenderId,
                        Content = msg.Content, AttachmentUrl = msg.AttachmentUrl,
                        SentAt = msg.Created.ToLocalTime(),
                        IsFromMe = isFromMe,
                        IsRead = msg.IsRead, IsSent = true,
                        ReplyToMessageId = msg.ReplyToMessageId,
                        ReplyToContent = msg.ReplyToContent,
                        ReplyToSenderName = msg.ReplyToSenderName
                    }
                });

                if (!msg.IsRead && msg.ReceiverId == _myUserId)
                {
                    _ = _messageService.MarkAsReadAsync(msg.Id);
                    if (_signalRService != null)
                        _ = _signalRService.MarkMessageReadAsync(msg.Id, msg.SenderId);
                }
            }

            if (prepend)
            {
                for (int i = 0; i < items.Count; i++)
                    Messages.Insert(i, items[i]);
            }
            else
            {
                foreach (var item in items)
                    Messages.Add(item);
            }
        }

        private async Task PerformSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { SearchResults.Clear(); return; }
            try
            {
                IsLoading = true;
                SearchResults.Clear();
                var result = await _searchService.SearchAsync(SearchText.Trim());
                if (result != null)
                {
                    foreach (var user in result.Users.Take(20)) SearchResults.Add(user);
                    foreach (var message in result.Messages.Take(20)) SearchResults.Add(message);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Qidiruv xatoligi: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SelectUserFromSearch(UserSearchResult? user)
        {
            if (user == null) return;
            var existingChat = Chats.FirstOrDefault(c => c.Id == user.Id);
            if (existingChat != null)
            {
                SelectedChat = existingChat;
            }
            else
            {
                var newChat = new ChatItem
                {
                    Id = user.Id, Name = user.FullName, ProfilePicture = user.ProfilePicture,
                    IsOnline = user.IsOnline, LastMessage = "", LastMessageTime = DateTime.Now, UnreadCount = 0, IsGroup = false
                };
                Chats.Insert(0, newChat);
                SelectedChat = newChat;
            }
            SearchText = string.Empty;
            SearchResults.Clear();
            IsSearching = false;
        }

        private void SelectMessageResult(MessageSearchResult? msgResult)
        {
            if (msgResult == null) return;
            var existingChat = Chats.FirstOrDefault(c => c.Id == msgResult.UserId);
            if (existingChat != null)
            {
                SelectedChat = existingChat;
            }
            else
            {
                var newChat = new ChatItem
                {
                    Id = msgResult.UserId, Name = msgResult.FullName, ProfilePicture = msgResult.ProfilePicture,
                    LastMessage = msgResult.MessageContent, LastMessageTime = msgResult.SentAt, UnreadCount = 0, IsGroup = false
                };
                Chats.Insert(0, newChat);
                SelectedChat = newChat;
            }
            SearchText = string.Empty;
            SearchResults.Clear();
            IsSearching = false;
        }

        private bool CanSendMessage() =>
            SelectedChat != null && !string.IsNullOrWhiteSpace(MessageText) && !IsLoading;

        private async Task SendMessageAsync()
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(MessageText)) return;
            try
            {
                IsLoading = true;
                var replyTo = ReplyingTo;
                var request = new SendMessageRequest
                {
                    ReceiverId = SelectedChat.Id,
                    Content = MessageText.Trim(),
                    ReplyToMessageId = replyTo?.Message?.Id
                };
                var success = await _messageService.SendMessageAsync(request);

                if (success)
                {
                    var now = DateTime.Now;
                    var messageDate = now.Date;
                    var lastItem = Messages.LastOrDefault();
                    if (lastItem != null && !lastItem.IsDateSeparator && lastItem.Message != null)
                    {
                        if (lastItem.Message.SentAt.Date != messageDate)
                            Messages.Add(new MessageItem { IsDateSeparator = true, DateText = FormatDateSeparator(messageDate) });
                    }
                    else if (lastItem == null)
                    {
                        Messages.Add(new MessageItem { IsDateSeparator = true, DateText = FormatDateSeparator(messageDate) });
                    }

                    Messages.Add(new MessageItem
                    {
                        IsDateSeparator = false,
                        Message = new Message
                        {
                            Id = DateTime.Now.Ticks, ChatId = SelectedChat.Id,
                            Content = MessageText.Trim(), SentAt = now,
                            IsFromMe = true, IsRead = false, IsSent = true,
                            ReplyToMessageId = replyTo?.Message?.Id,
                            ReplyToContent = replyTo?.Message?.Content ?? replyTo?.Message?.AttachmentUrl,
                            ReplyToSenderName = replyTo?.Message?.IsFromMe == true ? "Siz" : SelectedChat.Name
                        }
                    });

                    StopTypingNow();
                    ReplyingTo = null;
                    var chat = SelectedChat;
                    chat.LastMessage = MessageText.Trim();
                    chat.LastMessageTime = now;
                    MessageText = string.Empty;

                    var index = Chats.IndexOf(chat);
                    if (index > 0) Chats.Move(index, 0);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Xabar yuborishda xatolik: {ex.Message}", NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteMessageAsync(MessageItem? item, bool deleteForBoth = false)
        {
            if (item == null || item.IsDateSeparator || item.Message == null) return;
            try
            {
                var success = await _messageService.DeleteMessageAsync(item.Message.Id, deleteForBoth);
                if (success)
                {
                    Messages.Remove(item);
                    NotificationService.Show(deleteForBoth ? "Xabar ikki tomondan o'chirildi" : "Xabar o'chirildi", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Xabarni o'chirishda xatolik: {ex.Message}", NotificationType.Error);
            }
        }

        private void ShowClearChatDialog(ChatItem? chat)
        {
            if (chat == null || ShowClearChatDialogFunc == null) return;
            var (confirmed, deleteForBoth) = ShowClearChatDialogFunc(chat);
            if (confirmed) _ = ClearChatAsync(chat, deleteForBoth);
        }

        private async Task ClearChatAsync(ChatItem? chat, bool clearForBoth)
        {
            if (chat == null) return;
            try
            {
                var success = await _messageService.ClearChatAsync(chat.Id, clearForBoth);
                if (success)
                {
                    if (SelectedChat?.Id == chat.Id) Messages.Clear();
                    chat.LastMessage = "";
                    chat.UnreadCount = 0;
                    NotificationService.Show(clearForBoth ? "Chat ikki tomondan tozalandi" : "Chat tarixi tozalandi", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Chatni tozalashda xatolik: {ex.Message}", NotificationType.Error);
            }
        }

        public void ReceiveIncomingMessage(long senderId, string content, DateTime sentAtUtc, string? attachmentUrl = null)
        {
            var sentAt = sentAtUtc.ToLocalTime();
            var chat = Chats.FirstOrDefault(c => c.Id == senderId);
            if (chat == null) { _ = LoadChatsAsync(); return; }

            chat.LastMessage = content;
            chat.LastMessageTime = sentAt;
            chat.IsTyping = false;
            var index = Chats.IndexOf(chat);
            if (index > 0) Chats.Move(index, 0);
            _messageCache.Remove(senderId); // invalidate cache

            if (SelectedChat?.Id == senderId)
            {
                chat.UnreadCount = 0;
                IsRemoteTyping = false;
                var messageDate = sentAt.Date;
                var lastItem = Messages.LastOrDefault();
                bool needDateSep = lastItem == null
                    || lastItem.IsDateSeparator
                    || (lastItem.Message != null && lastItem.Message.SentAt.Date != messageDate);

                if (needDateSep && (lastItem == null || lastItem.Message?.SentAt.Date != messageDate))
                    Messages.Add(new MessageItem { IsDateSeparator = true, DateText = FormatDateSeparator(messageDate) });

                Messages.Add(new MessageItem
                {
                    IsDateSeparator = false,
                    Message = new Message
                    {
                        Id = sentAt.Ticks, ChatId = senderId, SenderId = senderId,
                        Content = content, AttachmentUrl = attachmentUrl, SentAt = sentAt,
                        IsFromMe = false, IsRead = true, IsSent = true
                    }
                });
            }
            else
            {
                chat.UnreadCount++;
                var senderName = chat.Name;
                var preview = !string.IsNullOrEmpty(content) ? content : "📷 Rasm";
                if (preview.Length > 60) preview = preview[..60] + "…";

                var isAppFocused = Application.Current.Dispatcher.Invoke(() =>
                    Application.Current.Windows.OfType<Window>().Any(w => w.IsActive));

                if (!isAppFocused)
                    NotificationService.ShowWindowsToast(senderName, preview);
                else
                    NotificationService.Show($"{senderName}: {preview}", NotificationType.Info, 4000);
            }
        }

        public void SetSignalRService(SignalRService service)
        {
            _signalRService = service;

            service.TypingStarted += senderId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var typingChat = Chats.FirstOrDefault(c => c.Id == senderId);
                    if (typingChat != null) typingChat.IsTyping = true;
                    if (SelectedChat?.Id != senderId) return;
                    IsRemoteTyping = true;
                    if (_remoteTypingTimer == null)
                    {
                        _remoteTypingTickHandler = (_, __) => { _remoteTypingTimer!.Stop(); IsRemoteTyping = false; };
                        _remoteTypingTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
                        _remoteTypingTimer.Tick += _remoteTypingTickHandler;
                    }
                    _remoteTypingTimer.Stop();
                    _remoteTypingTimer.Start();
                });
            };

            service.TypingStopped += senderId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var typingChat = Chats.FirstOrDefault(c => c.Id == senderId);
                    if (typingChat != null) typingChat.IsTyping = false;
                    if (SelectedChat?.Id == senderId) { _remoteTypingTimer?.Stop(); IsRemoteTyping = false; }
                });
            };

            service.MessageRead += messageId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Mark all sent messages as read (server marks them all when chat is opened)
                    foreach (var item in Messages)
                    {
                        if (item.Message != null && item.Message.IsFromMe && !item.Message.IsRead)
                        {
                            item.Message.IsRead = true;
                            item.NotifyMessageChanged();
                        }
                    }
                });
            };

            service.UserConnected += userId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var chat = Chats.FirstOrDefault(c => c.Id == userId);
                    if (chat != null) chat.IsOnline = true;
                });
            };

            service.UserDisconnected += userId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var chat = Chats.FirstOrDefault(c => c.Id == userId);
                    if (chat != null) { chat.IsOnline = false; chat.LastActive = DateTime.Now; }
                });
            };
        }

        private void HandleTypingIndicator()
        {
            if (_signalRService == null || SelectedChat == null) return;
            if (string.IsNullOrEmpty(MessageText)) { StopTypingNow(); return; }

            if (!_isTyping)
            {
                _isTyping = true;
                _ = _signalRService.SendTypingAsync(SelectedChat.Id);
            }

            if (_typingStopTimer == null)
            {
                _typingStopTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                _typingStopTimer.Tick += async (_, __) =>
                {
                    _typingStopTimer!.Stop();
                    _isTyping = false;
                    if (SelectedChat != null) await _signalRService.StopTypingAsync(SelectedChat.Id);
                };
            }
            _typingStopTimer.Stop();
            _typingStopTimer.Start();
        }

        private void StopTypingNow()
        {
            if (!_isTyping) return;
            _typingStopTimer?.Stop();
            _isTyping = false;
            if (_signalRService != null && SelectedChat != null)
                _ = _signalRService.StopTypingAsync(SelectedChat.Id);
        }

        private void Logout()
        {
            _authService.ClearToken();
            _navigation.ShowLoginView();
            RequestClose?.Invoke();
        }

        private static async Task<BitmapSource?> LoadBitmapAsync(string url)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);
                return await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    using var stream = new System.IO.MemoryStream(bytes);
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return (BitmapSource)bitmap;
                });
            }
            catch
            {
                return null;
            }
        }

        private static string GetFullImageUrl(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://")) return path;
            return $"http://localhost:5237/{path.TrimStart('/')}";
        }

        public void InvalidateChatCache(long chatId) => _messageCache.Remove(chatId);

        private void BeginEditMessage(MessageItem? item)
        {
            if (item?.Message == null || !item.Message.IsFromMe || item.Message.HasImage) return;
            item.Message.EditText = item.Message.Content;
            item.Message.IsEditing = true;
            item.NotifyMessageChanged();
        }

        private void CancelEdit(MessageItem? item)
        {
            if (item?.Message == null) return;
            item.Message.IsEditing = false;
            item.NotifyMessageChanged();
        }

        private async Task ConfirmEditAsync(MessageItem? item)
        {
            if (item?.Message == null || string.IsNullOrWhiteSpace(item.Message.EditText)) return;
            var newText = item.Message.EditText.Trim();
            if (newText == item.Message.Content) { CancelEdit(item); return; }
            try
            {
                var success = await _messageService.EditMessageAsync(item.Message.Id, newText);
                if (success)
                {
                    item.Message.Content = newText;
                    item.Message.IsEdited = true;
                    item.Message.IsEditing = false;
                    item.NotifyMessageChanged();
                    if (SelectedChat != null) InvalidateChatCache(SelectedChat.Id);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Tahrirlashda xatolik: {ex.Message}", NotificationType.Error);
            }
        }

        private static string FormatDateSeparator(DateTime date)
        {
            var today = DateTime.Today;
            if (date.Date == today) return "Bugun";
            if (date.Date == today.AddDays(-1)) return "Kecha";
            return date.ToString("d-MMMM");
        }
    }
}
