using Application.DTOs.Search;
using Application.DTOs.User;
using Application.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public AuthController(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            var validationResult = ValidateRequest<SignupRequest>(request);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                   .Select(e => e.ErrorMessage)
                   .ToArray();
                return BadRequest(AuthResponseDTO<AuthResult>.FromError(validationErrors));
            }

            var result = await _authService.SignupAsync(request.Username, request.Email, request.Password);
            if (!result.Success)
                return BadRequest(AuthResponseDTO<AuthResult>.FromError(result.ErrorMessage));

            return Ok(AuthResponseDTO<AuthResult>.FromSuccess(result));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validationResult = ValidateRequest<LoginRequest>(request);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(AuthResponseDTO<AuthResult>.FromError(validationErrors));
            }
               

            var result = await _authService.LoginAsync(request.Email, request.Password);
            if (!result.Success)
                return Unauthorized(AuthResponseDTO<AuthResult>.FromError(result.ErrorMessage));

            return Ok(AuthResponseDTO<AuthResult>.FromSuccess(result));
        }


        private ValidationResult ValidateRequest<T>(T request)
        {
            var validator = _serviceProvider.GetRequiredService<IValidator<T>>();
            return validator.Validate(request);
        }
    }
}
