using Application.DTOs.RandomJoke;
using Application.DTOs.Search;
using Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/jokes")]
    [Authorize]
    public class JokesController : ControllerBase
    {
        private readonly IJokeService _jokeService;
        private readonly IJokeSearchService _searchService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JokesController> _logger;

        public JokesController(
            IJokeService jokeService,
            IJokeSearchService searchService,
            IServiceProvider serviceProvider,
            ILogger<JokesController> logger)
        {
            _jokeService = jokeService;
            _searchService = searchService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet("random")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JokeDTO>> GetRandomJoke()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            _logger.LogInformation("User {UserId} ({Username}) fetching a random joke...", userId, username);
            _logger.LogInformation("Fetching a random joke...");
            var joke = await _jokeService.GetRandomJokeAsync();
            _logger.LogInformation("Successfully fetched random joke with ID {JokeId}", joke.Id);
            return Ok(joke);
        }

        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GroupedJokesDTO>> SearchJokes([FromBody] SearchRequestDTO searchRequest)
        {

            _logger.LogInformation("Search request received with query: {@SearchRequest}", searchRequest);

            var validator = _serviceProvider.GetRequiredService<IValidator<SearchRequestDTO>>();
            var validationResult = await validator.ValidateAsync(searchRequest);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for search request: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var result = await _searchService.SearchJokesAsync(searchRequest);
            _logger.LogInformation("Search completed successfully for query: {@SearchRequest}", searchRequest);
            return Ok(result);
        }
    }
}
