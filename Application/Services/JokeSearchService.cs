using Application.DTOs;
using Application.DTOs.Search;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class JokeSearchService : IJokeSearchService
    {
        private readonly IJokeRepository _repository;
        private readonly IJokeApiClient _apiClient;
        private readonly IJokeClassifier _classifier;
        private readonly IJokeHighlighter _highlighter;
        private readonly ICacheService _cacheService;

        public JokeSearchService(
            IJokeRepository repository,
            IJokeApiClient apiClient,
            IJokeClassifier classifier,
            IJokeHighlighter highlighter,
            ICacheService cacheService)
        {
            _repository = repository;
            _apiClient = apiClient;
            _classifier = classifier;
            _highlighter = highlighter;
            _cacheService = cacheService;
        }

        public async Task<GroupedJokesDTO> SearchJokesAsync(SearchRequestDTO searchRequest)
        {
            string cacheKey = GenerateCacheKey(searchRequest);
            var cachedResult = _cacheService.Get<GroupedJokesDTO>(cacheKey);

            if (cachedResult != null)
            {
                await _repository.TrackSearchTermAsync(searchRequest.Term);
                return cachedResult;
            }

            var allJokes = await FetchJokesFromDbAndApiAsync(searchRequest);
            
            await _repository.TrackSearchTermAsync(searchRequest.Term);

            var jokesWithHighlight = ApplyHighlighting(allJokes, searchRequest.Term);

            var result = GroupAndFormatJokes(jokesWithHighlight);
            _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15));
            return result;

        }

        private async Task<List<Joke>> FetchJokesFromDbAndApiAsync(SearchRequestDTO searchRequest)
        {
            var dbJokes = await _repository.SearchJokesAsync(searchRequest.Term, searchRequest.Limit);
            var allJokes = new List<Joke>(dbJokes);

            if (dbJokes.Count < searchRequest.Limit)
            {
                var newJokes = await FetchAndProcessApiJokesAsync(searchRequest, dbJokes);

                if (newJokes.Count != 0)
                {
                    await _repository.SaveJokesBatchAsync(newJokes, searchRequest.Term);
                    allJokes.AddRange(newJokes);
                }
            }

            return allJokes;
        }

        private async Task<List<Joke>> FetchAndProcessApiJokesAsync(SearchRequestDTO searchRequest, List<Joke> dbJokes)
        {
            var apiResponse = await _apiClient.SearchJokesAsync(searchRequest.Term, searchRequest.Limit, searchRequest.Page);// no subtraction as the api could get jokes we already have.

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
                        LastAccessedAt = DateTime.UtcNow
                    };

                    joke.WordCount = _classifier.CountWords(joke.JokeText);
                    joke.JokeLength = _classifier.ClassifyJoke(joke.JokeText);

                    newJokes.Add(joke);
                }
            }
            return newJokes;
        }

        private List<Joke> ApplyHighlighting(List<Joke> allJokes, string term)
        {
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
