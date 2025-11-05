using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class GroupedJokesDTO
    {
        public List<JokeDTO> Short { get; set; } = new List<JokeDTO>();
        public List<JokeDTO> Medium { get; set; } = new List<JokeDTO>();
        public List<JokeDTO> Long { get; set; } = new List<JokeDTO>();
    }
}
