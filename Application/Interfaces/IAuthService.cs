using Application.DTOs.User;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> SignupAsync(string username, string email, string password);
        Task<AuthResult> LoginAsync(string email, string password);
    }
}
