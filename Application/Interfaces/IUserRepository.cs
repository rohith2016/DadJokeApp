using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailHashAsync(string emailHash);
        Task<bool> ExistsAsync(string emailHash, string username);
        Task<int> CreateAsync(User user);
    }
}
