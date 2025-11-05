using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Search
{
    public class SearchRequestDTO
    {
        public required string Term { get; set; }
        public int Limit { get; set; } = 30;
        public int Page { get; set; } = 1;
    }
}
