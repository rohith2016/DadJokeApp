using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly UserRepositoryHelper _sqlHelper;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration config, UserRepositoryHelper helper, ILogger<UserRepository> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
            _sqlHelper = helper;
            _logger = logger;
        }

        public async Task<User?> GetByEmailHashAsync(string emailHash)
        {
            _logger.LogInformation("Fetching user by email hash '{EmailHash}'", emailHash);
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(_sqlHelper.GetByEmailHashSql(), connection);
                command.Parameters.AddWithValue("@EmailHash", emailHash);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var user = new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        EmailHash = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4)
                    };
                    _logger.LogInformation("User found with ID {UserId}", user.Id);
                    return user;
                }
                _logger.LogInformation("No user found with email hash '{EmailHash}'", emailHash);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching user by email hash '{EmailHash}'", emailHash);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string emailHash, string username)
        {
            _logger.LogInformation("Checking existence of user with email hash '{EmailHash}' or username '{Username}'", 
                emailHash, username);
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(_sqlHelper.ExistsWithEmailOrUsernameSql(), connection);
                command.Parameters.AddWithValue("@EmailHash", emailHash);
                command.Parameters.AddWithValue("@Username", username);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                var exists = count > 0;
                _logger.LogInformation("User existence check result: {Exists}", exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking user existence for email hash '{EmailHash}' and username '{Username}'", 
                    emailHash, username);
                throw;
            }
        }

        public async Task<int> CreateAsync(User user)
        {
            _logger.LogInformation("Creating new user with username '{Username}'", user.Username);
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(_sqlHelper.CreateUserSql(), connection);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@EmailHash", user.EmailHash);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

                var userId = Convert.ToInt32(await command.ExecuteScalarAsync());
                _logger.LogInformation("Successfully created user with ID {UserId}", userId);
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating user with username '{Username}'", user.Username);
                throw;
            }
        }
    }
}
