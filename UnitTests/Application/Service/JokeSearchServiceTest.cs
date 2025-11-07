using Application.DTOs;
using Application.DTOs.Search;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.Application.Service
{
    public class JokeSearchServiceTests
    {
        private readonly Mock<IJokeRepository> _repositoryMock;
        private readonly Mock<IJokeApiClient> _apiClientMock;
        private readonly Mock<IJokeClassifier> _classifierMock;
        private readonly Mock<IJokeHighlighter> _highlighterMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<JokeSearchService>> _loggerMock;
        private readonly JokeSearchService _service;

        public JokeSearchServiceTests()
        {
            _repositoryMock = new Mock<IJokeRepository>();
            _apiClientMock = new Mock<IJokeApiClient>();
            _classifierMock = new Mock<IJokeClassifier>();
            _highlighterMock = new Mock<IJokeHighlighter>();
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<JokeSearchService>>();

            _service = new JokeSearchService(
                _repositoryMock.Object,
                _apiClientMock.Object,
                _classifierMock.Object,
                _highlighterMock.Object,
                _cacheServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SearchJokesAsync_WhenCacheHit_ReturnsCachedResult()
        {
            var request = new SearchRequestDTO { Term = "dad", Limit = 5 };
            var cached = new GroupedJokesDTO { TotalJokes = 3 };

            _cacheServiceMock.Setup(c => c.Get<GroupedJokesDTO>(It.IsAny<string>()))
                .Returns(cached);

            var result = await _service.SearchJokesAsync(request);

            Assert.Equal(3, result.TotalJokes);
            _repositoryMock.Verify(r => r.TrackSearchTermAsync("dad"), Times.Once);
            _apiClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SearchJokesAsync_WhenCacheMiss_UsesDbOnly()
        {
            var request = new SearchRequestDTO { Term = "funny", Limit = 2 };

            _cacheServiceMock.Setup(c => c.Get<GroupedJokesDTO>(It.IsAny<string>()))
                .Returns((GroupedJokesDTO)null);

            var dbJokes = new List<Joke>
            {
                new Joke { Id = Guid.NewGuid(), JokeId = "1", JokeText = "funny joke", JokeLength = JokeLength.Short },
                new Joke { Id = Guid.NewGuid(), JokeId = "2", JokeText = "funny joke 2", JokeLength = JokeLength.Short }

            };

            _repositoryMock.Setup(r => r.SearchJokesAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(dbJokes);

            _highlighterMock.Setup(h => h.HighlightTerm(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string text, string term) => text.Replace(term, $"<b>{term}</b>", StringComparison.OrdinalIgnoreCase));

            var result = await _service.SearchJokesAsync(request);

            Assert.Equal(2, result.TotalJokes);
            _repositoryMock.Verify(r => r.SaveJokesBatchAsync(It.IsAny<List<Joke>>(), It.IsAny<string>()), Times.Never);
            _cacheServiceMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<GroupedJokesDTO>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task SearchJokesAsync_WhenCacheMiss_AndDbInsufficient_FetchesFromApi()
        {
            var request = new SearchRequestDTO { Term = "dog", Limit = 2, Page = 1 };
            _cacheServiceMock.Setup(c => c.Get<GroupedJokesDTO>(It.IsAny<string>())).Returns((GroupedJokesDTO)null);

            var dbJokes = new List<Joke>(); // Empty DB
            _repositoryMock.Setup(r => r.SearchJokesAsync("dog", 2)).ReturnsAsync(dbJokes);

            var apiResponse = new DadJokeSearchResponse
            {
                Results = new List<DadJoke>
                {
                    new DadJoke { Id = "j1", Joke = "Dog joke" }
                }
            };

            _apiClientMock.Setup(a => a.SearchJokesAsync("dog", 2, 1))
                .ReturnsAsync(apiResponse);

            _classifierMock.Setup(c => c.ClassifyJoke(It.IsAny<string>())).Returns(JokeLength.Short);
            _classifierMock.Setup(c => c.CountWords(It.IsAny<string>())).Returns(2);
            _highlighterMock.Setup(h => h.HighlightTerm(It.IsAny<string>(), It.IsAny<string>())).Returns((string s1, string s2) => s1);

            var result = await _service.SearchJokesAsync(request);

            Assert.Equal(1, result.TotalJokes);
            _repositoryMock.Verify(r => r.SaveJokesBatchAsync(It.IsAny<List<Joke>>(), "dog"), Times.Once);
        }

        [Fact]
        public async Task SearchJokesAsync_WhenRepositoryThrows_ThrowsException()
        {
            var request = new SearchRequestDTO { Term = "error", Limit = 3 };
            _cacheServiceMock.Setup(c => c.Get<GroupedJokesDTO>(It.IsAny<string>())).Returns((GroupedJokesDTO)null);

            _repositoryMock.Setup(r => r.SearchJokesAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB down"));

            await Assert.ThrowsAsync<Exception>(() => _service.SearchJokesAsync(request));
        }
    }
}
