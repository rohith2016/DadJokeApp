namespace Application.DTOs.User
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
