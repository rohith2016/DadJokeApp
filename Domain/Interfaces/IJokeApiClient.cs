using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IJokeApiClient
    {
        Task<DadJokeApiResponse> GetRandomJokeAsync();
        Task<DadJokeSearchResponse> SearchJokesAsync(string searchTerm, int limit, int page);
    }
}
