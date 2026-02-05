using System.Collections.Generic;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Auth
{
    public interface IAuthService
    {
        Task<Usuario?> AutenticarAsync(string username, string password);
        Task<Usuario> RegisterAsync(string firstName, string lastName, string username, string password, List<int> unitIds);
        Task DesconectarAsync();
        Task<Usuario?> GetCurrentUserAsync();
        Task UpdateUserAsync(Usuario user);
    }
}
