using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    internal class JokeSearchService : IJokeSearchService
    {
        public Task<GroupedJokesDTO> SearchJokesAsync(string term)
        {
            throw new NotImplementedException();
        }
    }
}
