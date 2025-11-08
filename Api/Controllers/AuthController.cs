using Application.DTOs.Search;
using Application.DTOs.User;
using Application.Interfaces;
using FluentValidation;
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
            var validator = _serviceProvider.GetRequiredService<IValidator<SignupRequest>>();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var result = await _authService.SignupAsync(request.Username, request.Email, request.Password);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { result.Token, result.UserName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(new { result.Token, result.UserName });
        }
    }
}
