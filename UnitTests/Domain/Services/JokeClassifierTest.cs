using Domain.Enum;
using Domain.Services;

namespace UnitTests.Domain.Services
{
    public class JokeClassifierTests
    {
        private readonly JokeClassifier _classifier;

        public JokeClassifierTests()
        {
            _classifier = new JokeClassifier();
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("   ", 0)]
        [InlineData("One", 1)]
        [InlineData("Two words", 2)]
        [InlineData("This is a test\nwith newlines", 6)]
        [InlineData("Tabs\tand spaces", 3)]
        public void CountWords_ReturnsExpectedCount(string input, int expectedCount)
        {
            var result = _classifier.CountWords(input);
            Assert.Equal(expectedCount, result);
        }

        [Theory]
        [InlineData("Hi", JokeLength.Short)] // 1 word
        [InlineData("One two three four five six seven eight nine", JokeLength.Short)] // 9 words
        [InlineData("This has ten words exactly in this short funny line", JokeLength.Medium)] // 10 words
        [InlineData("This joke has more than twenty words so it should be classified as long category because it goes on and on and on until it passes the threshold", JokeLength.Long)]
        public void ClassifyJoke_ReturnsExpectedLength(string jokeText, JokeLength expected)
        {
            var result = _classifier.ClassifyJoke(jokeText);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CountWords_WhenNull_ReturnsZero()
        {
            var result = _classifier.CountWords(null);
            Assert.Equal(0, result);
        }

        [Fact]
        public void ClassifyJoke_WhenEmpty_ReturnsShort()
        {
            var result = _classifier.ClassifyJoke("");
            Assert.Equal(JokeLength.Short, result);
        }
    }
}
