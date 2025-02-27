using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels;

namespace Kabutar_WPF.Views.Pages
{
    public partial class VerifyEmailPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly IAuthService _authService;
        private readonly string _email;

        public VerifyEmailPage(MainWindow mainWindow, IAuthService authService, string email)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _authService = authService;
            _email = email;
            DataContext = new VerifyEmailViewModel(_mainWindow, _authService, _email);
        }

        private void DigitTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void DigitTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.Key == Key.Back) 
                {
                    MoveToPreviousTextBox(textBox);
                }
                else if (textBox.Text.Length == 1) 
                {
                    MoveToNextTextBox(textBox);
                }
            }
        }

        private void MoveToNextTextBox(TextBox textBox)
        {
            var parent = textBox.Parent as StackPanel;
            if (parent == null) return;

            int index = parent.Children.IndexOf(textBox);
            if (index < parent.Children.Count - 1)
            {
                ((TextBox)parent.Children[index + 1]).Focus();
            }
        }

        private void MoveToPreviousTextBox(TextBox textBox)
        {
            var parent = textBox.Parent as StackPanel;
            if (parent == null) return;

            int index = parent.Children.IndexOf(textBox);
            if (index > 0)
            {
                ((TextBox)parent.Children[index - 1]).Focus();
            }
        }
    }
}
