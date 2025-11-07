using Application.DTOs;
using Application.DTOs.Search;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class JokeSearchService : IJokeSearchService
    {
        private readonly IJokeRepository _repository;
        private readonly IJokeApiClient _apiClient;
        private readonly IJokeClassifier _classifier;
        private readonly IJokeHighlighter _highlighter;
        private readonly ICacheService _cacheService;
        private readonly ILogger<JokeSearchService> _logger;

        public JokeSearchService(
            IJokeRepository repository,
            IJokeApiClient apiClient,
            IJokeClassifier classifier,
            IJokeHighlighter highlighter,
            ICacheService cacheService,
            ILogger<JokeSearchService> logger)
        {
            _repository = repository;
            _apiClient = apiClient;
            _classifier = classifier;
            _highlighter = highlighter;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<GroupedJokesDTO> SearchJokesAsync(SearchRequestDTO searchRequest)
        {
            try
            {
                _logger.LogInformation("Search started for term '{Term}' with limit {Limit}", searchRequest.Term, searchRequest.Limit);

                string cacheKey = GenerateCacheKey(searchRequest);
                var cachedResult = _cacheService.Get<GroupedJokesDTO>(cacheKey);

                if (cachedResult != null)
                {
                    _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
                    await _repository.TrackSearchTermAsync(searchRequest.Term);
                    return cachedResult;
                }

                _logger.LogInformation("Cache miss for key {CacheKey}, fetching from DB and API...", cacheKey);
                var allJokes = await FetchJokesFromDbAndApiAsync(searchRequest);

                await _repository.TrackSearchTermAsync(searchRequest.Term);

                var jokesWithHighlight = ApplyHighlighting(allJokes, searchRequest.Term);
                var result = GroupAndFormatJokes(jokesWithHighlight);

                _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15));
                _logger.LogInformation("Cache set for key {CacheKey} with {Count} total jokes", cacheKey, result.TotalJokes);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching jokes for term '{Term}'", searchRequest.Term);
                throw;
            }
        }

        private async Task<List<Joke>> FetchJokesFromDbAndApiAsync(SearchRequestDTO searchRequest)
        {
            try
            {
                _logger.LogInformation("Searching jokes in database for term '{Term}'", searchRequest.Term);
                var dbJokes = await _repository.SearchJokesAsync(searchRequest.Term, searchRequest.Limit);
                var allJokes = new List<Joke>(dbJokes);

                if (dbJokes.Count < searchRequest.Limit)
                {
                    _logger.LogInformation("Found {Count} jokes in DB, fetching more from API...", dbJokes.Count);
                    var newJokes = await FetchAndProcessApiJokesAsync(searchRequest, dbJokes);

                    if (newJokes.Count != 0)
                    {
                        _logger.LogInformation("Saving {Count} new jokes fetched from API", newJokes.Count);
                        await _repository.SaveJokesBatchAsync(newJokes, searchRequest.Term);
                        allJokes.AddRange(newJokes);
                    }
                }

                return allJokes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching jokes from DB or API for term '{Term}'", searchRequest.Term);
                throw;
            }
        }

        private async Task<List<Joke>> FetchAndProcessApiJokesAsync(SearchRequestDTO searchRequest, List<Joke> dbJokes)
        {
            _logger.LogInformation("Fetching jokes from API for term '{Term}' (limit {Limit}, page {Page})",
                searchRequest.Term, searchRequest.Limit, searchRequest.Page);

            var apiResponse = await _apiClient.SearchJokesAsync(searchRequest.Term, searchRequest.Limit, searchRequest.Page);
            var existingJokeIds = dbJokes.Select(j => j.JokeId).ToHashSet();
            var newJokes = new List<Joke>();

            foreach (var apiJoke in apiResponse.Results)
            {
                if (!existingJokeIds.Contains(apiJoke.Id))
                {
                    var joke = new Joke
                    {
                        Id = Guid.NewGuid(),
                        JokeId = apiJoke.Id,
                        JokeText = apiJoke.Joke,
                        CreatedAt = DateTime.UtcNow,
                        LastAccessedAt = DateTime.UtcNow,
                        WordCount = _classifier.CountWords(apiJoke.Joke),
                        JokeLength = _classifier.ClassifyJoke(apiJoke.Joke)
                    };

                    newJokes.Add(joke);
                }
            }

            _logger.LogInformation("Fetched {ApiCount} jokes from API, {NewCount} new jokes to save",
                apiResponse.Results.Count, newJokes.Count);

            return newJokes;
        }

        private List<Joke> ApplyHighlighting(List<Joke> allJokes, string term)
        {
            _logger.LogInformation("Applying highlighting for term '{Term}'", term);

            return allJokes
                .Select(j => new Joke
                {
                    Id = j.Id,
                    JokeId = j.JokeId,
                    JokeText = _highlighter.HighlightTerm(j.JokeText, term),
                    JokeLength = j.JokeLength,
                    WordCount = j.WordCount,
                    CreatedAt = j.CreatedAt,
                    LastAccessedAt = j.LastAccessedAt
                })
                .ToList();
        }

        private GroupedJokesDTO GroupAndFormatJokes(List<Joke> jokes)
        {
            _logger.LogInformation("Grouping {Count} jokes by length", jokes.Count);

            var grouped = jokes
                .GroupBy(j => j.JokeLength)
                .Select(g => new ClassifiedJokes
                {
                    LengthCategory = g.Key.ToString(),
                    Count = g.Count(),
                    Jokes = g.Select(j => new JokeDTO
                    {
                        Id = j.JokeId,
                        Text = j.JokeText,
                        Length = j.JokeLength.ToString()
                    }).ToList()
                })
                .ToList();

            return new GroupedJokesDTO
            {
                TotalJokes = jokes.Count,
                ClassifiedJokes = grouped
            };
        }

        private static string GenerateCacheKey(SearchRequestDTO searchRequest)
        {
            return $"search_{searchRequest.Term.Trim().ToLower()}_{searchRequest.Limit}";
        }
    }
}
