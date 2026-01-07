using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon;


namespace pwa_camera_poc_blazor.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly AwsConfig _awsConfig;
        private const string SESSION_KEY = "pwa-inventory-session";
        public string? AwsIdToken { get; private set; }

        public AuthService(ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, AwsConfig awsConfig)
        {
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _awsConfig = awsConfig;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            // Default admin user for testing
            if (username.ToLower().Trim() == "admin" && password == "admin")
            {
                var user = await _localStorage.GetItemAsync<User>("admin");
                if (user == null)
                {
                    user = new User
                    {
                        Username = "admin",
                        FirstName = "Administrador",
                        LastName = "do Sistema",
                        PasswordHash = "",
                        UnitIds = new List<int> { 1 },
                        CurrentUnitId = 1,
                        CreatedAt = DateTime.Now
                    };
                    await _localStorage.SetItemAsync("admin", user);
                }
                user.LastLogin = DateTime.Now;
                await _localStorage.SetItemAsync(user.Username, user);

                var session = new UserSession { Username = user.Username, UnitId = user.CurrentUnitId ?? 0 };
                await _localStorage.SetItemAsync(SESSION_KEY, session);

                if (_authStateProvider is CustomAuthStateProvider customProvider)
                {
                    customProvider.NotifyAuthenticationStateChanged();
                }

                return user;
            }

            var user2 = await _localStorage.GetItemAsync<User>(username.ToLower().Trim());
            if (user2 == null) return null;

            var inputHash = HashPassword(password);
            if (user2.PasswordHash != inputHash) return null;

            // AWS Cognito Flow (Simulated/Implementation)
            /* 
            // Commented out for PoC mode with static credentials
            try {
                if (!string.IsNullOrEmpty(_awsConfig.UserPoolId)) {
                    var provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), RegionEndpoint.GetBySystemName(_awsConfig.Region));
                    var userPool = new CognitoUserPool(_awsConfig.UserPoolId, _awsConfig.AppClientId, provider);
                    var cognitoUser = new CognitoUser(username, _awsConfig.AppClientId, userPool, provider);
                    
                    var authResponse = await cognitoUser.StartWithSrpAuthAsync(new InitiateSrpAuthRequest { Password = password });
                    AwsIdToken = authResponse.AuthenticationResult.IdToken;
                }
            } catch (Exception ex) {
                Console.WriteLine($"Cognito Login Error: {ex.Message}");
                // In a real scenario, we might fail here, but for POC let's continue if local auth passed
            }
            */

            user2.LastLogin = DateTime.Now;
            await _localStorage.SetItemAsync(user2.Username, user2);

            // Create session (minimal info)
            var session2 = new UserSession { Username = user2.Username, UnitId = user2.CurrentUnitId ?? 0 };
            await _localStorage.SetItemAsync(SESSION_KEY, session2);

            if (_authStateProvider is CustomAuthStateProvider customProvider2)
            {
                customProvider2.NotifyAuthenticationStateChanged();
            }

            return user2;
        }

        public async Task<User> RegisterAsync(string firstName, string lastName, string username, string password, List<int> unitIds)
        {
            var distinctIds = unitIds.Distinct().ToList();
            var user = new User
            {
                Username = username.ToLower().Trim(),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                PasswordHash = HashPassword(password),
                UnitIds = distinctIds,
                CurrentUnitId = distinctIds.Count > 0 ? distinctIds[0] : null,
                CreatedAt = DateTime.Now
            };

            await _localStorage.SetItemAsync(user.Username, user);
            return user;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(SESSION_KEY);
            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var session = await _localStorage.GetItemAsync<UserSession>(SESSION_KEY);
            if (session == null) return null;

            return await _localStorage.GetItemAsync<User>(session.Username);
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
        public async Task UpdateUserAsync(User user)
        {
            user.UnitIds = user.UnitIds.Distinct().ToList();
            await _localStorage.SetItemAsync(user.Username, user);

            // If the updated user is the current session user, update session as well
            var session = await _localStorage.GetItemAsync<UserSession>(SESSION_KEY);
            if (session != null && session.Username == user.Username)
            {
                session.UnitId = user.CurrentUnitId ?? 0;
                await _localStorage.SetItemAsync(SESSION_KEY, session);
            }
        }
    }

    public class UserSession
    {
        public string Username { get; set; } = string.Empty;
        public int UnitId { get; set; }
    }
}
