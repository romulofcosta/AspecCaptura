using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Text.Json.Serialization;

namespace pwa_camera_poc_blazor.Services.Auth
{
    public class AuthService(ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, IHttpClientFactory httpClientFactory, IIndexedDbService dbService, AppState appState) : IAuthService
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

                // Lê o response dinâmico
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse == null) return null;

                // Monta objeto Usuario para manter compatibilidade
                var user = new Usuario
                {
                    UsuarioNome = cleanUsername!,
                    NomeCompleto = loginResponse.NomeCompleto,
                    Prefixo = loginResponse.Prefixo,
                    Esfera = loginResponse.Esfera,
                    Orgaos = loginResponse.Orgaos ?? new(),
                    Token = loginResponse.Token,
                    Patrimonio = new List<PatrimonioItem>() // será populado abaixo
                };

                // Persistência da esfera
                if (!string.IsNullOrEmpty(user.Esfera))
                {
                    await localStorage.SetItemAsync("user-esfera", user.Esfera);
                }

                // Persistência da carga de patrimônio recebida
                if (loginResponse.Patrimonio != null && loginResponse.Patrimonio.Count > 0)
                {
                    await dbService.InitializeAsync();
                    await dbService.ClearAsync("patrimonio");
                    foreach (var item in loginResponse.Patrimonio)
                    {
                        var patr = new PatrimonioItem
                        {
                            IdPatomb = item.IdPatomb,
                            Nutomb = item.Nutomb ?? string.Empty,
                            Esfera = item.Esfera ?? string.Empty,
                            Deprod = item.Deprod ?? string.Empty
                        };
                        await dbService.AddAsync("patrimonio", patr);
                        user.Patrimonio.Add(patr);
                    }
                }

                // Cache user data locally
                await localStorage.SetItemAsync(user.UsuarioNome, user);

                // Create session with esfera
                var session = new UserSession { Username = user.UsuarioNome, Esfera = user.Esfera };
                await localStorage.SetItemAsync(SESSION_KEY, session);

                // CORREÇÃO BUG: Limpar dados de sessão anterior no localStorage
                // Remove possíveis dados de configuração de sessão anterior
                await localStorage.RemoveItemAsync("session-config");

                // CORREÇÃO BUG: Limpar estado da sessão anterior no AppState
                // Isso garante que os dropdowns da ConfiguracaoSessao sejam resetados
                appState.ClearSessionData();
                appState.EsferaAtual = user.Esfera;

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
            await localStorage.RemoveItemAsync("session-config");
            
            // CORREÇÃO BUG: Limpar estado da sessão ao fazer logout
            appState.ClearSessionData();
            
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
        public string? Esfera { get; set; }
    }
}

// DTO para consumir o novo contrato da API
public class LoginResponse
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Prefixo { get; set; } = string.Empty;
    public string Esfera { get; set; } = string.Empty;
    public List<Orgao>? Orgaos { get; set; }
    public List<PatrimonioApiItem>? Patrimonio { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class PatrimonioApiItem
{
    [JsonPropertyName("idpatomb")]
    public long IdPatomb { get; set; }
    [JsonPropertyName("nutomb")]
    public string? Nutomb { get; set; }
    [JsonPropertyName("esfera")]
    public string? Esfera { get; set; }
    [JsonPropertyName("deprod")]
    public string? Deprod { get; set; }
}
