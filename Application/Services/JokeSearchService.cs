using Application.DTOs;
using Application.DTOs.Search;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class JokeSearchService : IJokeSearchService
    {
        private readonly IJokeRepository _repository;
        private readonly IJokeApiClient _apiClient;
        private readonly IJokeClassifier _classifier;
        private readonly IJokeHighlighter _highlighter;

        public JokeSearchService(
            IJokeRepository repository,
            IJokeApiClient apiClient,
            IJokeClassifier classifier,
            IJokeHighlighter highlighter)
        {
            _repository = repository;
            _apiClient = apiClient;
            _classifier = classifier;
            _highlighter = highlighter;
        }

        public async Task<GroupedJokesDTO> SearchJokesAsync(SearchRequestDTO searchRequest)
        {
            // Step 1: Search database first
            var dbJokes = await _repository.SearchJokesAsync(searchRequest.Term);
            var allJokes = new List<Joke>(dbJokes);

            // Step 2: If less than 30 jokes found, fetch from API
            if (dbJokes.Count < 30)
            {
                var apiResponse = await _apiClient.SearchJokesAsync(searchRequest.Term,searchRequest.Limit, searchRequest.Page);

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
            else
            {
                // Track search term even if we didn't hit API
                await _repository.TrackSearchTermAsync(searchRequest.Term);
            }

            // Step 4: Apply highlighting to all jokes
            var jokesWithHighlight = allJokes
                .Take(30)
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
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(j => new JokeDTO
                    {
                        Id = j.JokeId,
                        Text = j.JokeText,
                        Length = j.JokeLength.ToString()
                    }).ToList()
                );

            // Step 6: Return grouped results
            return new GroupedJokesDTO
            {
                Short = grouped.ContainsKey(JokeLength.Short)
                    ? grouped[JokeLength.Short]
                    : new List<JokeDTO>(),
                Medium = grouped.ContainsKey(JokeLength.Medium)
                    ? grouped[JokeLength.Medium]
                    : new List<JokeDTO>(),
                Long = grouped.ContainsKey(JokeLength.Long)
                    ? grouped[JokeLength.Long]
                    : new List<JokeDTO>()
            };
        }
    }
}
