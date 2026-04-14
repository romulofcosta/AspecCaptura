using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspecCaptura.Models;
using AspecCaptura.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Text.Json.Serialization;

namespace AspecCaptura.Services.Auth
{
    public class AuthService(ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, IHttpClientFactory httpClientFactory, IIndexedDbService dbService, AppState appState) : IAuthService
    {
        private const string SESSION_KEY = "pwa-inventory-session";
        private Usuario? _currentUser;
        private SessionToken? _currentToken;

        public event EventHandler<AuthStateChangedEventArgs>? OnAuthStateChanged;

        public bool IsAuthenticated => _currentUser != null;

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var result = new LoginResult { Success = false };
            var cleanUsername = username?.ToLower().Trim();

            try
            {
                var client = httpClientFactory.CreateClient("BackendApi");
                var response = await client.PostAsJsonAsync("/api/auth/login", new { Usuario = cleanUsername, Senha = password });

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResp = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                        result.ErrorMessage = errorResp?.Error ?? "Credenciais inválidas ou erro no servidor.";
                    }
                    catch
                    {
                        result.ErrorMessage = "Credenciais inválidas ou erro no servidor.";
                    }
                    return result;
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse == null)
                {
                    result.ErrorMessage = "Resposta inválida do servidor.";
                    return result;
                }

                var user = new Usuario
                {
                    UsuarioNome = cleanUsername!,
                    NomeCompleto = loginResponse.NomeCompleto,
                    Prefixo = loginResponse.Prefixo,
                    Esfera = loginResponse.Esfera,
                    Orgaos = loginResponse.Orgaos ?? new(),
                    Token = loginResponse.Token,
                    Patrimonio = new List<PatrimonioItem>()
                };

                if (!string.IsNullOrEmpty(user.Esfera))
                {
                    await localStorage.SetItemAsync("user-esfera", user.Esfera);
                }

                var received = loginResponse.Tombamentos ?? loginResponse.Patrimonio;
                // Ignora carga pesada no login: sincronização completa acontece em /tombamentos-sync

                // Do not store the full `Patrimonio` list in localStorage (can be very large).
                // Persist the user metadata only and keep patrimonio items in IndexedDB.
                var userToStore = new Usuario
                {
                    UsuarioNome = user.UsuarioNome,
                    NomeCompleto = user.NomeCompleto,
                    Prefixo = user.Prefixo,
                    Esfera = user.Esfera,
                    Orgaos = user.Orgaos ?? new(),
                    Token = user.Token,
                    Patrimonio = new List<PatrimonioItem>()
                };
                await localStorage.SetItemAsync(user.UsuarioNome, userToStore);

                var session = new UserSession { Username = user.UsuarioNome, Esfera = user.Esfera };
                await localStorage.SetItemAsync(SESSION_KEY, session);

                await localStorage.RemoveItemAsync("session-config");

                appState.ClearSessionData();
                appState.EsferaAtual = user.Esfera;
                appState.CurrentUser = user;
                appState.IsAuthenticated = true;

                _currentUser = user;
                _currentToken = new SessionToken { Token = user.Token ?? string.Empty, IssuedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(480), UserId = user.UsuarioNome };

                // Persistir o token para o CustomAuthStateProvider reconhecer após refresh
                await localStorage.SetItemAsync("auth_token", _currentToken);
                
                // Salvar o estado global para garantir persistência após refresh
                await appState.SaveStateAsync();

                if (authStateProvider is CustomAuthStateProvider customProvider)
                {
                    customProvider.NotifyAuthenticationStateChanged();
                    await Task.Delay(100);
                }

                OnAuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs { IsAuthenticated = true, User = user });

                result.Success = true;
                result.User = user;
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Authentication error: {ex.Message}");
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task LogoutAsync()
        {
            await localStorage.RemoveItemAsync(SESSION_KEY);
            await localStorage.RemoveItemAsync("session-config");
            await localStorage.RemoveItemAsync("auth_token");

            appState.ClearSessionData();

            _currentUser = null;
            _currentToken = null;

            if (authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }

            OnAuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs { IsAuthenticated = false, User = null });
        }

        public async Task<Usuario?> GetCurrentUserAsync()
        {
            var session = await localStorage.GetItemAsync<UserSession>(SESSION_KEY);
            if (session == null) return null;

            var user = await localStorage.GetItemAsync<Usuario>(session.Username);
            _currentUser = user;
            return user;
        }

        public Usuario? GetCurrentUser()
        {
            return _currentUser;
        }

        public SessionToken? GetCurrentToken()
        {
            if (_currentToken != null) return _currentToken;
            if (_currentUser == null || string.IsNullOrEmpty(_currentUser.Token)) return null;

            _currentToken = new SessionToken
            {
                Token = _currentUser.Token,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                UserId = _currentUser.UsuarioNome
            };
            return _currentToken;
        }

        public async Task<bool> ValidateTokenAsync()
        {
            var token = GetCurrentToken();
            return token?.IsValid ?? false;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var client = httpClientFactory.CreateClient("BackendApi");
                var token = GetCurrentToken();
                if (token == null) return false;

                var response = await client.PostAsJsonAsync("/api/auth/refresh", new { token = token.Token });
                if (!response.IsSuccessStatusCode) return false;

                var newToken = await response.Content.ReadFromJsonAsync<SessionToken>();
                if (newToken == null) return false;

                _currentToken = newToken;
                if (_currentUser != null)
                {
                    _currentUser.Token = newToken.Token;
                    await localStorage.SetItemAsync(_currentUser.UsuarioNome, _currentUser);
                }

                return true;
            }
            catch
            {
                return false;
            }
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
            // Avoid storing the full patrimonio collection in localStorage to prevent quota errors.
            var userToStore = new Usuario
            {
                UsuarioNome = user.UsuarioNome,
                NomeCompleto = user.NomeCompleto,
                Prefixo = user.Prefixo,
                Esfera = user.Esfera,
                Orgaos = user.Orgaos ?? new(),
                Token = user.Token,
                Patrimonio = new List<PatrimonioItem>()
            };
            await localStorage.SetItemAsync(user.UsuarioNome, userToStore);
            _currentUser = user;

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
        public string? Esfera { get; set; }
    }
}

// DTO para consumir o novo contrato da API
public class ApiErrorResponse
{
    public string? Error { get; set; }
    public string? Detail { get; set; }
}

public class LoginResponse
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Prefixo { get; set; } = string.Empty;
    public string Esfera { get; set; } = string.Empty;
    public List<Orgao>? Orgaos { get; set; }
    // Backend returns 'tombamentos' (see API); keep 'Patrimonio' for backward compat
    [System.Text.Json.Serialization.JsonPropertyName("tombamentos")]
    public List<PatrimonioApiItem>? Tombamentos { get; set; }
    public List<PatrimonioApiItem>? Patrimonio { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class PatrimonioApiItem
{
    [JsonPropertyName("idPatomb")]
    public long IdPatomb { get; set; }
    [JsonPropertyName("nutomb")]
    public string? Nutomb { get; set; }
    [JsonPropertyName("esfera")]
    public string? Esfera { get; set; }
    [JsonPropertyName("deprod")]
    public string? Deprod { get; set; }
}
