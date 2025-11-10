using Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Helpers
{
    public class UserRepositoryHelper
    {
        private readonly string _userTable;

        public UserRepositoryHelper(IConfiguration configuration)
        {
            _userTable = configuration["UsersTable"] ?? "users";
        }
        public string GetByEmailHashSql()
        {
            return $"SELECT id, username, email, password_hash, created_at FROM {_userTable} WHERE email = @EmailHash";
        }

        public string ExistsWithEmailOrUsernameSql()
        {
            return $"SELECT COUNT(*) FROM {_userTable} WHERE email = @EmailHash OR username = @Username";
        }

        public string CreateUserSql()
        {
            return @$"INSERT INTO {_userTable} (username, email, password_hash) 
                    VALUES (@Username, @EmailHash, @PasswordHash) 
                    RETURNING id";
        }
    }
}
