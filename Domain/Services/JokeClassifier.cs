using Domain.Enum;
using Domain.Interfaces;

namespace Domain.Services
{
    public class JokeClassifier: IJokeClassifier
    {
        private static readonly char[] separator = new[] { ' ', '\t', '\n', '\r' };

        public JokeLength ClassifyJoke(string jokeText)
        {
            var wordCount = CountWords(jokeText);

            if (wordCount < 10)
            {
                return JokeLength.Short;
            }
            else if (wordCount < 20)
            {
                return JokeLength.Medium;
            }
            else
            {
                return JokeLength.Long;
            }
        }

        public int CountWords(string jokeText)
        {
            if (string.IsNullOrWhiteSpace(jokeText))
            {
                return 0;
            }

            var words = jokeText.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }
    }
}
