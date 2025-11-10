using Domain.Enum;

namespace Domain.Entities
{
    public class Joke
    {
        public Guid Id { get; set; }
        public string JokeId { get; set; } = string.Empty;
        public string JokeText { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public JokeLength JokeLength { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
    }
}
