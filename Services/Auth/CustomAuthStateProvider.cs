using System; // Required for Exception and Console
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using AspecCaptura.Models; // For implicit usage if needed, but session is UserSession
using AspecCaptura.Services.Storage;

namespace AspecCaptura.Services.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private const string SESSION_KEY = "pwa-inventory-session";

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>(SESSION_KEY);
                if (session == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var token = await _localStorage.GetItemAsync<SessionToken>("auth_token");
                if (token == null || !token.IsValid)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, session.Username)
                };

                var identity = new ClaimsIdentity(claims, "Custom Auth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CustomAuthStateProvider: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
