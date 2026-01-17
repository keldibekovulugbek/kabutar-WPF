using System;
using System.Windows;
using Microsoft.Win32;
using Kabutar_WPF.Models.Users;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.Views
{
    public partial class ProfileSettingsView : Window
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private string? _selectedImagePath;

        public ProfileSettingsView(IUserService userService, IAuthService authService)
        {
            InitializeComponent();
            _userService = userService;
            _authService = authService;

            LoadCurrentUserData();
        }

        private async void LoadCurrentUserData()
        {
            try
            {
                // For now, we'll get user ID from auth service
                var userId = _authService.GetUserId();
                if (userId == null)
                {
                    MessageBox.Show("Foydalanuvchi ma'lumotlari topilmadi.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // TODO: Load current user data from API
                // var userProfile = await _userService.GetCurrentUserAsync();
                // For now, leave fields empty for user to fill
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ma'lumotlarni yuklashda xatolik: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Profil rasmini tanlang",
                Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                SelectedImagePath.Text = System.IO.Path.GetFileName(_selectedImagePath);
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                {
                    MessageBox.Show("Ism kiritilishi shart.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
                    FirstNameTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                {
                    MessageBox.Show("Familiya kiritilishi shart.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LastNameTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                {
                    MessageBox.Show("Username kiritilishi shart.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UsernameTextBox.Focus();
                    return;
                }

                // Disable button to prevent double-click
                var saveButton = sender as System.Windows.Controls.Button;
                if (saveButton != null)
                    saveButton.IsEnabled = false;

                // Update profile
                var updateRequest = new UserUpdateRequest
                {
                    Firstname = FirstNameTextBox.Text.Trim(),
                    Lastname = LastNameTextBox.Text.Trim(),
                    Username = UsernameTextBox.Text.Trim(),
                    About = AboutTextBox.Text.Trim()
                };

                var success = await _userService.UpdateProfileAsync(updateRequest);

                if (!success)
                {
                    MessageBox.Show("Profilni yangilashda xatolik.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (saveButton != null)
                        saveButton.IsEnabled = true;
                    return;
                }

                // Upload image if selected
                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    try
                    {
                        await _userService.UploadProfileImageAsync(_selectedImagePath);
                    }
                    catch (Exception imgEx)
                    {
                        MessageBox.Show($"Rasmni yuklashda xatolik: {imgEx.Message}\n\nLekin boshqa ma'lumotlar saqlandi.", "Ogohlantirish", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show("Profil muvaffaqiyatli yangilandi!", "Muvaffaqiyat", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xatolik yuz berdi: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);

                // Re-enable button
                var saveButton = sender as System.Windows.Controls.Button;
                if (saveButton != null)
                    saveButton.IsEnabled = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
