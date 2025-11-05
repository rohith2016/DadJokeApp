using Domain.Enum;

namespace Domain.Entities
{
    public class Joke
    {
        public Guid Id { get; set; }
        public string JokeId { get; set; }
        public string JokeText { get; set; }
        public int WordCount { get; set; }
        public JokeLength JokeLength { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
    }
}
