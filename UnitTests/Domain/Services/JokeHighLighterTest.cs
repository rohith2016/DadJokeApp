using Domain.Services;

namespace UnitTests.Domain.Services
{
    public class JokeHighlighterTests
    {
        private readonly JokeHighlighter _highlighter;

        public JokeHighlighterTests()
        {
            _highlighter = new JokeHighlighter();
        }

        [Theory]
        [InlineData("Dad jokes are funny", "Dad", "<b>Dad</b> jokes are funny")]
        [InlineData("Dad jokes are funny", "jokes", "Dad <b>jokes</b> are funny")]
        [InlineData("Funny dad joke, dad!", "dad", "Funny <b>dad</b> joke, <b>dad</b>!")]
        [InlineData("Funny Dad Joke", "dad", "Funny <b>Dad</b> Joke")] // Case-insensitive
        public void HighlightTerm_HighlightsMatchingTerms(string text, string term, string expected)
        {
            var result = _highlighter.HighlightTerm(text, term);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", "dad")]
        [InlineData("Funny dad joke", "")]
        [InlineData(null, "dad")]
        [InlineData("Funny dad joke", null)]
        public void HighlightTerm_WhenEmptyOrNull_ReturnsOriginal(string text, string term)
        {
            var result = _highlighter.HighlightTerm(text, term);
            Assert.Equal(text, result);
        }

        [Fact]
        public void HighlightTerm_WhenNoMatch_ReturnsUnchanged()
        {
            var text = "Funny dad joke";
            var term = "cat";

            var result = _highlighter.HighlightTerm(text, term);

            Assert.Equal(text, result);
        }

        [Fact]
        public void HighlightTerm_MultipleOccurrences_AllHighlighted()
        {
            var text = "dad dad dad";
            var term = "dad";
            var expected = "<b>dad</b> <b>dad</b> <b>dad</b>";

            var result = _highlighter.HighlightTerm(text, term);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HighlightTerm_PartialWord_DoesNotSkipAny()
        {
            var text = "daddy dad";
            var term = "dad";
            var expected = "<b>dad</b>dy <b>dad</b>";

            var result = _highlighter.HighlightTerm(text, term);

            Assert.Equal(expected, result);
        }
    }
}
