using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResult> SignupAsync(string username, string email, string password)
        {
            var emailHash = HashEmail(email);

            if (await _userRepository.ExistsAsync(emailHash, username))
            {
                return new AuthResult { Success = false, ErrorMessage = "User already exists" };
            }

            var user = new User
            {
                Username = username,
                EmailHash = emailHash,
                PasswordHash = HashPassword(password)
            };

            var userId = await _userRepository.CreateAsync(user);
            user.Id = userId;

            var token = _jwtTokenGenerator.GenerateToken(userId, username, email);

            return new AuthResult
            {
                Success = true,
                Token = token,
                UserName = username
            };
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var emailHash = HashEmail(email);
            var user = await _userRepository.GetByEmailHashAsync(emailHash);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid credentials" };
            }

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, email);

            return new AuthResult
            {
                Success = true,
                Token = token,
                UserName = user.Username
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string HashEmail(string email)
        {
            using var sha256 = SHA256.Create();
            var emailBytes = Encoding.UTF8.GetBytes(email.ToLowerInvariant());
            var hashBytes = sha256.ComputeHash(emailBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
