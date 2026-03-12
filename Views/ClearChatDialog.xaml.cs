using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Kabutar_WPF.Models.Chat;

namespace Kabutar_WPF.Views
{
    public partial class ClearChatDialog : Window
    {

        public bool Confirmed { get; private set; }


        public bool DeleteForBoth => AlsoDeleteCheckBox.IsChecked == true;

        public ClearChatDialog(ChatItem chat, Window owner)
        {
            InitializeComponent();
            Owner = owner;


            SubText.Text = $"{chat.Name} bilan bo'lgan barcha xabar tarixi o'chirilsinmi?";


            CheckboxLabel.Text = $"{chat.Name} uchun ham o'chirish";


            var photoPath = chat.ThumbnailOrProfile;
            if (!string.IsNullOrEmpty(photoPath))
            {
                try
                {
                    string url = photoPath;
                    if (!url.StartsWith("http://") && !url.StartsWith("https://") && !(url.Length >= 2 && url[1] == ':'))
                        url = $"http://localhost:5237/{url.TrimStart('/')}";

                    PhotoBrush.ImageSource = new BitmapImage(new Uri(url));
                    PhotoBorder.Visibility = Visibility.Visible;
                    InitialsBorder.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    SetInitials(chat.Name);
                }
            }
            else
            {
                SetInitials(chat.Name);
            }
        }

        private void SetInitials(string name)
        {
            InitialsText.Text = GetInitials(name);
            InitialsBorder.Visibility = Visibility.Visible;
            PhotoBorder.Visibility = Visibility.Collapsed;
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
            return name.Length >= 2
                ? name[..2].ToUpper()
                : name.ToUpper();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}
