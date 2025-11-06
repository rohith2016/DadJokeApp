using Domain.Interfaces;

namespace Domain.Services
{
    public class JokeHighlighter : IJokeHighlighter
    {
        public string HighlightTerm(string jokeText, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(jokeText) || string.IsNullOrWhiteSpace(searchTerm))
            {
                return jokeText;
            }

            var startIndex = 0;
            var result = jokeText;
            var termLength = searchTerm.Length;
            var offset = 0;

            while (startIndex < jokeText.Length)
            {
                var index = jokeText.IndexOf(searchTerm, startIndex, StringComparison.OrdinalIgnoreCase);

                if (index == -1)
                {
                    break;
                }

                var originalTerm = jokeText.Substring(index, termLength);
                var highlighted = $"<b>{originalTerm}</b>";

                result = result.Remove(index + offset, termLength);
                result = result.Insert(index + offset, highlighted);

                offset += 7; // Length of "<b>" + "</b>"
                startIndex = index + termLength;
            }

            return result;
        }
    }
}
