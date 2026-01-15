using System;
using System.Windows.Controls;

namespace Kabutar_WPF.Core
{
    /// <summary>
    /// Service for managing page navigation
    /// </summary>
    public class NavigationService
    {
        private static NavigationService? _instance;
        private Frame? _navigationFrame;

        public static NavigationService Instance => _instance ??= new NavigationService();

        public Frame? NavigationFrame
        {
            get => _navigationFrame;
            set => _navigationFrame = value;
        }

        public void NavigateTo(Page page)
        {
            if (_navigationFrame != null)
            {
                _navigationFrame.Navigate(page);
            }
        }

        public void NavigateTo<T>() where T : Page, new()
        {
            if (_navigationFrame != null)
            {
                _navigationFrame.Navigate(new T());
            }
        }

        public void GoBack()
        {
            if (_navigationFrame?.CanGoBack == true)
            {
                _navigationFrame.GoBack();
            }
        }

        public void GoForward()
        {
            if (_navigationFrame?.CanGoForward == true)
            {
                _navigationFrame.GoForward();
            }
        }

        public void ClearNavigationHistory()
        {
            if (_navigationFrame != null)
            {
                while (_navigationFrame.CanGoBack)
                {
                    _navigationFrame.RemoveBackEntry();
                }
            }
        }
    }
}
