namespace Domain.Interfaces
{
    public interface IJokeHighlighter
    {
        string HighlightTerm(string jokeText, string searchTerm);
    }
}
