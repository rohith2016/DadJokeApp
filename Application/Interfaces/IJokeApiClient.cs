using Domain.Models;

namespace Application.Interfaces
{
    public interface IJokeApiClient
    {
        Task<DadJokeApiResponse> GetRandomJokeAsync();
        Task<DadJokeSearchResponse> SearchJokesAsync(string searchTerm, int limit, int page);
    }
}
