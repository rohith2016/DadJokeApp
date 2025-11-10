using Application.DTOs.RandomJoke;

namespace Application.Interfaces
{
    public interface IJokeService
    {
        Task<JokeDTO> GetRandomJokeAsync();
    }
}
