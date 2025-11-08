using Api.Controllers;
using Application.DTOs.RandomJoke;
using Application.DTOs.Search;
using Application.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.Api.Controllers
{
    public class JokesControllerTests
    {
        private readonly Mock<IJokeService> _jokeServiceMock;
        private readonly Mock<IJokeSearchService> _searchServiceMock;
        private readonly Mock<IValidator<SearchRequestDTO>> _validatorMock;
        private readonly Mock<ILogger<JokesController>> _loggerMock;
        private readonly ServiceProvider _serviceProvider;

        public JokesControllerTests()
        {
            _jokeServiceMock = new Mock<IJokeService>();
            _searchServiceMock = new Mock<IJokeSearchService>();
            _validatorMock = new Mock<IValidator<SearchRequestDTO>>();
            _loggerMock = new Mock<ILogger<JokesController>>();

            var services = new ServiceCollection();
            services.AddSingleton(_validatorMock.Object);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task GetRandomJoke_ReturnsOk_WithJoke()
        {
            // Arrange
            var expectedJoke = new JokeDTO { Id = Guid.NewGuid().ToString(), Text = "Funny joke" };
            _jokeServiceMock.Setup(s => s.GetRandomJokeAsync()).ReturnsAsync(expectedJoke);

            var controller = new JokesController(
                _jokeServiceMock.Object,
                _searchServiceMock.Object,
                _serviceProvider,
                _loggerMock.Object);

            // Act
            var result = await controller.GetRandomJoke();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var joke = Assert.IsType<JokeDTO>(okResult.Value);
            Assert.Equal(expectedJoke.Id, joke.Id);
        }

        [Fact]
        public async Task GetRandomJoke_WhenException_LogsErrorAndReturns500()
        {
            _jokeServiceMock.Setup(s => s.GetRandomJokeAsync())
                .ThrowsAsync(new Exception("DB error"));

            var controller = new JokesController(
                _jokeServiceMock.Object,
                _searchServiceMock.Object,
                _serviceProvider,
                _loggerMock.Object);

            var result = await controller.GetRandomJoke();

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
        }

        [Fact]
        public async Task SearchJokes_ValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var request = new SearchRequestDTO { Term = "bad" };
            var mockValidationResult = new ValidationResult();
            mockValidationResult.Errors.Add(new ValidationFailure("Term", "Invalid query"));
            _validatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(mockValidationResult);

            var controller = new JokesController(
                _jokeServiceMock.Object,
                _searchServiceMock.Object,
                _serviceProvider,
                _loggerMock.Object);

            // Act
            var result = await controller.SearchJokes(request);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badResult.Value);
            Assert.Contains("Invalid query", errors);
        }

        [Fact]
        public async Task SearchJokes_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new SearchRequestDTO { Term = "friend" };
            _validatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());

            var expectedResult = new GroupedJokesDTO();
            _searchServiceMock.Setup(s => s.SearchJokesAsync(request)).ReturnsAsync(expectedResult);

            var controller = new JokesController(
                _jokeServiceMock.Object,
                _searchServiceMock.Object,
                _serviceProvider,
                _loggerMock.Object);

            // Act
            var result = await controller.SearchJokes(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<GroupedJokesDTO>(okResult.Value);
        }

        [Fact]
        public async Task SearchJokes_WhenException_Returns500()
        {
            var request = new SearchRequestDTO { Term = "friend" };
            _validatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());
            _searchServiceMock.Setup(s => s.SearchJokesAsync(request))
                .ThrowsAsync(new Exception("Search error"));

            var controller = new JokesController(
                _jokeServiceMock.Object,
                _searchServiceMock.Object,
                _serviceProvider,
                _loggerMock.Object);

            var result = await controller.SearchJokes(request);

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
        }
    }
}
