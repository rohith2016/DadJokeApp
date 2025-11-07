using Application.DTOs;
using Application.DTOs.Search;
using Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/jokes")]
    public class JokesController : ControllerBase
    {
        private readonly IJokeService _jokeService;
        private readonly IJokeSearchService _searchService;
        private readonly IServiceProvider _serviceProvider;

        public JokesController(
            IJokeService jokeService,
            IJokeSearchService searchService,
            IServiceProvider serviceProvider)
        {
            _jokeService = jokeService;
            _searchService = searchService;
            _serviceProvider = serviceProvider;
        }

        [HttpGet("random")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JokeDTO>> GetRandomJoke()
        {
            JokeDTO joke = await _jokeService.GetRandomJokeAsync();
            return Ok(joke);
        }

        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GroupedJokesDTO>> SearchJokes([FromBody] SearchRequestDTO searchRequest)
        {
            var validator = _serviceProvider.GetRequiredService<IValidator<SearchRequestDTO>>();
            var validationResult = await validator.ValidateAsync(searchRequest);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            
            GroupedJokesDTO result = await _searchService.SearchJokesAsync(searchRequest);
            return Ok(result);
        }
    }
}
