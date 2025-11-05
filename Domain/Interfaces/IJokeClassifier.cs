using Domain.Enum;

namespace Domain.Interfaces
{
    public interface IJokeClassifier
    {
        JokeLength ClassifyJoke(string jokeText);
        int CountWords(string jokeText);
    }
}
