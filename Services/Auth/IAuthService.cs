using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Auth
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string username, string password);
        Task<User> RegisterAsync(string username, string password, string firstName, string lastName, List<int> unitIds);
        Task LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        Task UpdateUserAsync(User user);
    }
}
