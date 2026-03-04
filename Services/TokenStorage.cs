using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kabutar_WPF.Services
{
    public static class TokenStorage
    {
        private static readonly string TokenFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kabutar",
            "auth.dat"
        );

        public static void SaveToken(string token)
        {
            try
            {
                var directory = Path.GetDirectoryName(TokenFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var encryptedData = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(token),
                    null,
                    DataProtectionScope.CurrentUser
                );

                File.WriteAllBytes(TokenFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving token: {ex.Message}");
            }
        }

        public static string? LoadToken()
        {
            try
            {
                if (!File.Exists(TokenFilePath))
                    return null;

                var encryptedData = File.ReadAllBytes(TokenFilePath);

                var decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    null,
                    DataProtectionScope.CurrentUser
                );

                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading token: {ex.Message}");
                return null;
            }
        }

        public static void ClearToken()
        {
            try
            {
                if (File.Exists(TokenFilePath))
                {
                    File.Delete(TokenFilePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing token: {ex.Message}");
            }
        }

        public static bool HasToken()
        {
            var token = LoadToken();
            return !string.IsNullOrEmpty(token);
        }
    }
}
