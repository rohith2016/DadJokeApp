using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<User?> GetByEmailHashAsync(string emailHash)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT id, username, email, password_hash, created_at FROM users WHERE email = @EmailHash",
                connection);
            command.Parameters.AddWithValue("@EmailHash", emailHash);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    EmailHash = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                };
            }
            return null;
        }

        public async Task<bool> ExistsAsync(string emailHash, string username)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT COUNT(*) FROM users WHERE email = @EmailHash OR username = @Username",
                connection);
            command.Parameters.AddWithValue("@EmailHash", emailHash);
            command.Parameters.AddWithValue("@Username", username);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"INSERT INTO users (username, email, password_hash) 
                  VALUES (@Username, @EmailHash, @PasswordHash) 
                  RETURNING id",
                connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@EmailHash", user.EmailHash);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

            var userId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(userId);
        }
    }
}
