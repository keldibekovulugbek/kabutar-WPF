using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels;
using Kabutar_WPF.ViewModels.Auth;
using Kabutar_WPF.Views;
using Kabutar_WPF.Views.Auth;

namespace Kabutar_WPF.Core
{
    public class WindowNavigationService : IWindowNavigationService
    {
        private readonly IAuthService _authService;
        private readonly ISearchService _searchService;
        private readonly IMessageService _messageService;
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        public WindowNavigationService(
            IAuthService authService,
            ISearchService searchService,
            IMessageService messageService,
            IChatService chatService,
            IUserService userService)
        {
            _authService = authService;
            _searchService = searchService;
            _messageService = messageService;
            _chatService = chatService;
            _userService = userService;
        }

        public void ShowMainView()
        {
            var vm = new MainViewModel(_authService, _searchService, _messageService, _chatService, _userService, this);
            var view = new MainView(vm);
            view.Show();
        }

        public void ShowLoginView()
        {
            var vm = new LoginViewModel(_authService, this);
            var view = new LoginView(vm);
            view.Show();
        }

        public void ShowRegisterView()
        {
            var vm = new RegisterViewModel(_authService, this);
            var view = new RegisterView(vm);
            view.Show();
        }

        public void ShowVerifyEmailView(string email)
        {
            var vm = new VerifyEmailViewModel(_authService, this) { Email = email };
            var view = new VerifyEmailView(vm);
            view.Show();
        }

        public void ShowForgotPasswordView()
        {
            var vm = new ForgotPasswordViewModel(_authService, this);
            var view = new ForgotPasswordView(vm);
            view.Show();
        }
    }
}
