using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class JokeService : IJokeService
    {
        private readonly IJokeApiClient _apiClient;
        private readonly IJokeClassifier _classifier;
        private readonly IJokeRepository _repository;
        private readonly ILogger<JokeService> _logger;

        public JokeService(
            IJokeApiClient apiClient,
            IJokeClassifier classifier,
            IJokeRepository repository,
            ILogger<JokeService> logger)
        {
            _apiClient = apiClient;
            _classifier = classifier;
            _repository = repository;
            _logger = logger;
        }

        public async Task<JokeDTO> GetRandomJokeAsync()
        {
            try
            {
                _logger.LogInformation("Fetching random joke from API...");
                var apiResponse = await _apiClient.GetRandomJokeAsync();

                var joke = new Joke
                {
                    Id = Guid.NewGuid(),
                    JokeId = apiResponse.Id,
                    JokeText = apiResponse.Joke,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow
                };

                joke.JokeLength = _classifier.ClassifyJoke(joke.JokeText);
                joke.WordCount = _classifier.CountWords(joke.JokeText);

                _logger.LogInformation("Fetched joke: {JokeId}, length: {Length}, words: {WordCount}",
                    joke.JokeId, joke.JokeLength, joke.WordCount);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation("Saving joke {JokeId} in the background...", joke.JokeId);
                        await _repository.SaveJokeAsync(joke);
                        _logger.LogInformation("Joke {JokeId} saved successfully.", joke.JokeId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving joke {JokeId} to repository.", joke.JokeId);
                    }
                });

                return new JokeDTO
                {
                    Id = joke.JokeId,
                    Text = joke.JokeText,
                    Length = joke.JokeLength.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching or processing random joke.");
                throw;
            }
        }
    }
}
