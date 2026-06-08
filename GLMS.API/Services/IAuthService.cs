using GLMS.API.Models;

namespace GLMS.API.Services
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string username, string password);
        Task<bool> RegisterUserAsync(string username, string email, string password);
    }
}