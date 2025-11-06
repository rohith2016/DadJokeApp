namespace Application.DTOs
{
    public class GroupedJokesDTO
    {
        public int TotalJokes { get; set; } = 0;
        public List<ClassifiedJokes> ClassifiedJokes { get; set; } = new List<ClassifiedJokes>();
    }

    public class ClassifiedJokes
    {
        public string LengthCategory { get; set; } = string.Empty;
        public int Count { get; set; } 
        public List<JokeDTO> Jokes { get; set; } = new List<JokeDTO>();
    }
}
