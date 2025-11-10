namespace Application.DTOs.User
{
    public class AuthResponseDTO<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static AuthResponseDTO<T> FromSuccess(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static AuthResponseDTO<T> FromError(params string[] errors) => new()
        {
            Success = false,
            Errors = errors
        };
    }
}
