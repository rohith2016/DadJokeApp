using Domain.Interfaces;
using Infrastructure.ExternalService.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalService
{
    public class DadJokeApiClient : IJokeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DadJokeApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            var baseUrl = _configuration["DadJokeApi:BaseUrl"] ?? "https://icanhazdadjoke.com/";
            var timeout = _configuration.GetValue<int>("DadJokeApi:Timeout", 30);

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DadJokeApp (https://github.com/yourapp)");
        }

        public async Task<DadJokeApiResponse> GetRandomJokeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                response.EnsureSuccessStatusCode();

                var joke = await response.Content.ReadFromJsonAsync<DadJokeApiResponse>();

                if (joke == null)
                {
                    throw new HttpRequestException("Failed to deserialize joke response");
                }

                return joke;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error fetching random joke from API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Request to dad joke API timed out", ex);
            }
        }

        public async Task<DadJokeSearchResponse> SearchJokesAsync(string searchTerm, int limit, int page)
        {
            try
            {
                var term = Uri.EscapeDataString(searchTerm);
                var response = await _httpClient.GetAsync($"search?term={term}&limit={limit}&page={page}");
                response.EnsureSuccessStatusCode();

                var searchResult = await response.Content.ReadFromJsonAsync<DadJokeSearchResponse>();

                if (searchResult == null)
                {
                    throw new HttpRequestException("Failed to deserialize search response");
                }

                return searchResult;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error searching jokes from API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Request to dad joke API timed out", ex);
            }
        }
    }
}
