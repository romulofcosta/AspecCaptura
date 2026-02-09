using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using pwa_camera_poc_blazor.Services.Provisioning;
using Microsoft.AspNetCore.Components.Authorization;

namespace pwa_camera_poc_blazor.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private const string SESSION_KEY = "pwa-inventory-session";

        public AuthService(ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<Usuario?> AutenticarAsync(string username, string password)
        {
            var cleanUsername = username?.ToLower().Trim();
            if (string.IsNullOrEmpty(cleanUsername)) return null;

            // Default admin user for testing
            if (cleanUsername == "admin" && password == "admin")
            {
                var user = await _localStorage.GetItemAsync<Usuario>("admin");
                if (user == null)
                {
                    user = new Usuario
                    {
                        NomeUsuario = "admin",
                        PrimeiroNome = "Administrador",
                        UltimoNome = "do Sistema",
                        HashSenha = "",
                        IdsUnidadesGestoras = new List<int> { 1 },
                        UnidadeGestoraAtualId = 1,
                        DataCriacao = DateTime.Now
                    };
                    await _localStorage.SetItemAsync("admin", user);
                }
                user.UltimoLogin = DateTime.Now;
                await _localStorage.SetItemAsync(user.NomeUsuario, user);

                var session = new UserSession { Username = user.NomeUsuario, UnitId = user.UnidadeGestoraAtualId ?? 0 };
                await _localStorage.SetItemAsync(SESSION_KEY, session);

                if (_authStateProvider is CustomAuthStateProvider customProvider)
                {
                    customProvider.NotifyAuthenticationStateChanged();
                }

                return user;
            }

            var user2 = await _localStorage.GetItemAsync<Usuario>(cleanUsername);
            if (user2 == null) return null;

            var inputHash = HashPassword(password);
            if (user2.HashSenha != inputHash) return null;

            user2.UltimoLogin = DateTime.Now;
            await _localStorage.SetItemAsync(user2.NomeUsuario, user2);

            // Create session (minimal info)
            var session2 = new UserSession { Username = user2.NomeUsuario, UnitId = user2.UnidadeGestoraAtualId ?? 0 };
            await _localStorage.SetItemAsync(SESSION_KEY, session2);

            if (_authStateProvider is CustomAuthStateProvider customProvider2)
            {
                customProvider2.NotifyAuthenticationStateChanged();
            }

            return user2;
        }

        public async Task<Usuario> RegisterAsync(string firstName, string lastName, string username, string password, List<int> unitIds)
        {
            var distinctIds = unitIds.Distinct().ToList();
            var user = new Usuario
            {
                NomeUsuario = username.ToLower().Trim(),
                PrimeiroNome = firstName.Trim(),
                UltimoNome = lastName.Trim(),
                HashSenha = HashPassword(password),
                IdsUnidadesGestoras = distinctIds,
                UnidadeGestoraAtualId = distinctIds.Count > 0 ? distinctIds[0] : null,
                DataCriacao = DateTime.Now
            };

            await _localStorage.SetItemAsync(user.NomeUsuario, user);
            return user;
        }

        public async Task DesconectarAsync()
        {
            await _localStorage.RemoveItemAsync(SESSION_KEY);
            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }
        }

        public async Task<Usuario?> GetCurrentUserAsync()
        {
            var session = await _localStorage.GetItemAsync<UserSession>(SESSION_KEY);
            if (session == null) return null;

            return await _localStorage.GetItemAsync<Usuario>(session.Username);
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
            user.IdsUnidadesGestoras = user.IdsUnidadesGestoras.Distinct().ToList();
            await _localStorage.SetItemAsync(user.NomeUsuario, user);

            // If the updated user is the current session user, update session as well
            var session = await _localStorage.GetItemAsync<UserSession>(SESSION_KEY);
            if (session != null && session.Username == user.NomeUsuario)
            {
                session.UnitId = user.UnidadeGestoraAtualId ?? 0;
                await _localStorage.SetItemAsync(SESSION_KEY, session);
            }
        }

        /// <summary>
        /// Downloads and syncs users for a UnidadeGestora from provisioning service.
        /// Stores users in IndexedDB with Argon2id password hashes.
        /// Called after authentication to enable offline auth for current UG.
        /// </summary>
        public async Task SincronizarUsuariosUGAsync(int ugId, IProvisioningService provisioningService)
        {
            try
            {
                var usuarios = await provisioningService.DownloadUsuariosAsync(ugId);
                foreach (var user in usuarios)
                {
                    await _localStorage.SetItemAsync(user.NomeUsuario, user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] Error syncing users: {ex.Message}");
            }
        }

        /// <summary>
        /// Authenticates user using Argon2id password hash stored in provisioned users JSON.
        /// Enables 100% offline authentication after initial provisioning.
        /// Validates hash locally without server call.
        /// </summary>
        public async Task<Usuario?> AutenticarComArgon2IdAsync(string username, string argon2IdHash)
        {
            try
            {
                var cleanUsername = username?.ToLower().Trim();
                if (string.IsNullOrEmpty(cleanUsername)) return null;

                var user = await _localStorage.GetItemAsync<Usuario>(cleanUsername);
                if (user == null) return null;

                // Validate against stored Argon2id hash
                if (!ValidateArgon2IdHash(argon2IdHash, user.HashSenha))
                {
                    return null;
                }

                // Authentication successful
                user.UltimoLogin = DateTime.Now;
                await _localStorage.SetItemAsync(user.NomeUsuario, user);

                var session = new UserSession { Username = user.NomeUsuario, UnitId = user.UnidadeGestoraAtualId ?? 0 };
                await _localStorage.SetItemAsync(SESSION_KEY, session);

                if (_authStateProvider is CustomAuthStateProvider customProvider)
                {
                    customProvider.NotifyAuthenticationStateChanged();
                }

                return user;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] Error in Argon2id auth: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validates Argon2id password hash.
        /// Implementation stub - requires Argon2id validation library.
        /// Recommended library: Isopoh.Cryptography.Argon2 or argon2-core-dotnet
        /// 
        /// Production implementation should:
        /// - Use Argon2 parameters: m=65536, t=3, p=4
        /// - Return true if hash matches plaintext password
        /// </summary>
        private bool ValidateArgon2IdHash(string plaintext, string argon2IdHash)
        {
            try
            {
                // TODO: Replace with actual Argon2id library
                // Example with Isopoh.Cryptography.Argon2:
                // return Argon2.Verify(argon2IdHash, plaintext, null);
                
                // Temporary fallback: simple hash comparison
                var tempHash = HashPassword(plaintext);
                return tempHash == argon2IdHash;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] Argon2id validation error: {ex.Message}");
                return false;
            }
        }
    }

    public class UserSession
    {
        public string Username { get; set; } = string.Empty;
        public int UnitId { get; set; }
    }
}
