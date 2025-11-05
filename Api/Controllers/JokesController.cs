using Application.DTOs;
using Application.DTOs.Search;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/jokes")]
    public class JokesController : ControllerBase
    {
        private readonly IJokeService _jokeService;
        private readonly IJokeSearchService _searchService;

        public JokesController(
            IJokeService jokeService,
            IJokeSearchService searchService)
        {
            _jokeService = jokeService;
            _searchService = searchService;
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
            if (string.IsNullOrWhiteSpace(searchRequest.Term))
            {
                return BadRequest("Search term is required");
            }

            if (searchRequest.Term.Length > 100)
            {
                return BadRequest("Search term too long");
            }

            GroupedJokesDTO result = await _searchService.SearchJokesAsync(searchRequest);
            return Ok(result);
        }
    }
}
