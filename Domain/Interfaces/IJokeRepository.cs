using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IJokeRepository
    {
        Task<List<Joke>> SearchJokesAsync(string searchTerm);
        Task SaveJokeAsync(Joke joke);
        Task SaveJokesBatchAsync(List<Joke> jokes, string searchTerm);
        Task TrackSearchTermAsync(string searchTerm);
    }
}
