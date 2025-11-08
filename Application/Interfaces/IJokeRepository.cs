using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJokeRepository
    {
        Task<List<Joke>> SearchJokesAsync(string searchTerm, int limit);
        Task SaveJokeAsync(Joke joke);
        Task SaveJokesBatchAsync(List<Joke> jokes, string searchTerm);
        Task TrackSearchTermAsync(string searchTerm);
    }
}
