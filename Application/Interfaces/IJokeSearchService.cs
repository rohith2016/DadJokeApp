using Application.DTOs;
using Application.DTOs.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IJokeSearchService
    {
        Task<GroupedJokesDTO> SearchJokesAsync(SearchRequestDTO searchRequest);
    }
}
