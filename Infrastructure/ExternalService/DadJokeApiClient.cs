using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.ExternalService
{
    public class DadJokeApiClient : IJokeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DadJokeApiClient> _logger;

        public DadJokeApiClient(HttpClient httpClient, ILogger<DadJokeApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DadJokeApiResponse> GetRandomJokeAsync()
        {
            try
            {
                _logger.LogInformation("Fetching random joke from Dad Joke API...");
                var response = await _httpClient.GetAsync("");
                response.EnsureSuccessStatusCode();

                var joke = await response.Content.ReadFromJsonAsync<DadJokeApiResponse>();

                if (joke == null)
                {
                    _logger.LogWarning("Failed to deserialize random joke response.");
                    throw new HttpRequestException("Failed to deserialize joke response");
                }

                _logger.LogInformation("Fetched random joke successfully with ID: {JokeId}", joke.Id);
                return joke;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching random joke from API.");
                throw new Exception($"Error fetching random joke from API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to Dad Joke API timed out.");
                throw new Exception("Request to Dad Joke API timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching random joke.");
                throw;
            }
        }

        public async Task<DadJokeSearchResponse> SearchJokesAsync(string searchTerm, int limit, int page)
        {
            try
            {
                var term = Uri.EscapeDataString(searchTerm);
                var url = $"search?term={term}&limit={limit}&page={page}";
               
                _logger.LogInformation("Searching jokes from DadJoke API with term '{Term}', limit {Limit}, page {Page}", searchTerm, limit, page);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var searchResult = await response.Content.ReadFromJsonAsync<DadJokeSearchResponse>();

                if (searchResult == null)
                {
                    _logger.LogWarning("Failed to deserialize search response for term '{Term}'", searchTerm);
                    throw new HttpRequestException("Failed to deserialize search response");
                }

                _logger.LogInformation("Fetched {Count} jokes for term '{Term}'", searchResult.Results?.Count ?? 0, searchTerm);
                return searchResult;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while searching jokes for term '{Term}'", searchTerm);
                throw new Exception($"Error searching jokes from API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to Dad Joke API timed out for term '{Term}'", searchTerm);
                throw new Exception("Request to Dad Joke API timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while searching jokes for term '{Term}'", searchTerm);
                throw;
            }
        }
    }
}
