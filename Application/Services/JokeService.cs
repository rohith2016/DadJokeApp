using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class JokeService : IJokeService
    {
        private readonly IJokeApiClient _apiClient;
        private readonly IJokeClassifier _classifier;
        private readonly IJokeRepository _repository;

        public JokeService(
            IJokeApiClient apiClient,
            IJokeClassifier classifier,
            IJokeRepository repository)
        {
            _apiClient = apiClient;
            _classifier = classifier;
            _repository = repository;
        }

        public async Task<JokeDTO> GetRandomJokeAsync()
        {
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

            _ = Task.Run(async () =>
            {
                try
                {
                    await _repository.SaveJokeAsync(joke);
                }
                catch
                {
                    //Logerror 
                }
            });

            return new JokeDTO
            {
                Id = joke.JokeId,
                Text = joke.JokeText,
                Length = joke.JokeLength.ToString()
            };
        }
    }
}
