using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Auth
{
    public interface IAuthService
    {
        Task<Usuario?> LoginAsync(string username, string password);

        Task LogoutAsync();
        Task<Usuario?> GetCurrentUserAsync();
        Task UpdateUserAsync(Usuario user);
    }
}
