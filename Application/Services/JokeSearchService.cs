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
            string cacheKey = $"search_{searchRequest.Term.Trim().ToLower()}_{searchRequest.Limit}";
            var cachedResult = _cacheService.Get<GroupedJokesDTO>(cacheKey);

            if (cachedResult != null)
            {
                await _repository.TrackSearchTermAsync(searchRequest.Term);
                return cachedResult;
            }

            // Step 1: Search database first
            var dbJokes = await _repository.SearchJokesAsync(searchRequest.Term, searchRequest.Limit);
            var allJokes = new List<Joke>(dbJokes);

            // Step 2: If less than the user choosen limit of jokes found, fetch from API
            if (dbJokes.Count < searchRequest.Limit)
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
                        allJokes.Add(joke);
                    }
                }

                // Step 3: Save new jokes to database
                if (newJokes.Any())
                {
                    await _repository.SaveJokesBatchAsync(newJokes, searchRequest.Term);
                }
            }
            // Track search term even if we didn't hit API
            await _repository.TrackSearchTermAsync(searchRequest.Term);

            // Step 4: Apply highlighting to all jokes
            var jokesWithHighlight = allJokes
                .Select(j => new Joke
                {
                    Id = j.Id,
                    JokeId = j.JokeId,
                    JokeText = _highlighter.HighlightTerm(j.JokeText, searchRequest.Term),
                    JokeLength = j.JokeLength,
                    WordCount = j.WordCount,
                    CreatedAt = j.CreatedAt,
                    LastAccessedAt = j.LastAccessedAt
                })
                .ToList();

            // Step 5: Group by length
            var grouped = jokesWithHighlight
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

            // Step 6: Return grouped results
            var result = new GroupedJokesDTO
            {
                TotalJokes = jokesWithHighlight.Count,
                ClassifiedJokes = grouped
            };

            _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15));
            return result;

        }
    }
}
