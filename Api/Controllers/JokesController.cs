using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult<JokeDto>> GetRandomJoke()
        {
            var joke = await _jokeService.GetRandomJokeAsync();
            return Ok(joke);
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GroupedJokesDto>> SearchJokes([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Search term is required");
            }

            if (term.Length > 100)
            {
                return BadRequest("Search term too long");
            }

            var result = await _searchService.SearchJokesAsync(term);
            return Ok(result);
        }
    }
}
