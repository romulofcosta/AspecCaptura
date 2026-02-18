using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;

namespace pwa_camera_poc_blazor.Services.Auth
{
    public class AuthService(ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, IHttpClientFactory httpClientFactory) : IAuthService
    {
        private const string SESSION_KEY = "pwa-inventory-session";

        public async Task<Usuario?> LoginAsync(string username, string password)
        {
            var cleanUsername = username?.ToLower().Trim();

            try
            {
                var client = httpClientFactory.CreateClient("BackendApi");
                var response = await client.PostAsJsonAsync("/api/auth/login", new { Usuario = cleanUsername, Senha = password });

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var user = await response.Content.ReadFromJsonAsync<Usuario>();
                if (user == null) return null;

                // Cache user data locally
                await localStorage.SetItemAsync(user.UsuarioNome, user);

                // Create session
                var session = new UserSession { Username = user.UsuarioNome };
                await localStorage.SetItemAsync(SESSION_KEY, session);

                if (authStateProvider is CustomAuthStateProvider customProvider)
                {
                    customProvider.NotifyAuthenticationStateChanged();
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Authentication error: {ex.Message}");
                return null;
            }
        }



        public async Task LogoutAsync()
        {
            await localStorage.RemoveItemAsync(SESSION_KEY);
            if (authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }
        }

        public async Task<Usuario?> GetCurrentUserAsync()
        {
            var session = await localStorage.GetItemAsync<UserSession>(SESSION_KEY);
            if (session == null) return null;

            return await localStorage.GetItemAsync<Usuario>(session.Username);
        }

        private string HashPassword(string password)
        {
            int hash = 0;
            string str = password + "SALT_KEY_PWA_2024";
            unchecked
            {
                for (int i = 0; i < str.Length; i++)
                {
                    int charCode = str[i];
                    hash = ((hash << 5) - hash) + charCode;
                }
            }
            return ToString36(hash);
        }

        private string ToString36(int value)
        {
            string charList = "0123456789abcdefghijklmnopqrstuvwxyz";
            if (value == 0) return "0";

            if (value == int.MinValue) return "-1z141z4";

            string result = "";
            bool negative = value < 0;
            long v = Math.Abs((long)value);

            while (v > 0)
            {
                result = charList[(int)(v % 36)] + result;
                v /= 36;
            }
            return negative ? "-" + result : result;
        }
        public async Task UpdateUserAsync(Usuario user)
        {
            await localStorage.SetItemAsync(user.UsuarioNome, user);

            // If the updated user is the current session user, update session as well
            var session = await localStorage.GetItemAsync<UserSession>(SESSION_KEY);
            if (session != null && session.Username == user.UsuarioNome)
            {
                await localStorage.SetItemAsync(SESSION_KEY, session);
            }
        }
    }

    public class UserSession
    {
        public string Username { get; set; } = string.Empty;
    }
}
