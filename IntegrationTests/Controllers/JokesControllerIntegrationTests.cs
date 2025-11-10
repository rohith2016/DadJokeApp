using Application.DTOs.RandomJoke;
using Application.DTOs.Search;
using Application.Interfaces;
using Domain.Models.Exceptions;
using Domain.Models.Exceptions.Domain.Models.Exceptions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace IntegrationTests.Controllers
{
    public class JokesControllerIntegrationTests
    {
        private readonly JsonSerializerOptions _jsonOptions = new() 
        { 
            PropertyNameCaseInsensitive = true 
        };

        [Fact]
        public async Task GetRandomJoke_WhenGeneralExceptionThrown_Returns500()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var mockJokeService = new Mock<IJokeService>();
                        mockJokeService.Setup(s => s.GetRandomJokeAsync())
                            .ThrowsAsync(new Exception("DB error"));

                        services.AddScoped(_ => mockJokeService.Object);
                    });
                });

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/jokes/random");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("An unexpected error occurred.", content);
        }

        [Fact]
        public async Task GetRandomJoke_WhenUnauthorizedExceptionThrown_Returns401()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var mockJokeService = new Mock<IJokeService>();
                        mockJokeService.Setup(s => s.GetRandomJokeAsync())
                            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized access"));

                        services.AddScoped(_ => mockJokeService.Object);
                    });
                });

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/jokes/random");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
            Assert.Equal("Unauthorized access", error!.Error);
        }

        [Fact]
        public async Task GetRandomJoke_WhenTooManyRequestsExceptionThrown_Returns429()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var mockJokeService = new Mock<IJokeService>();
                        mockJokeService.Setup(s => s.GetRandomJokeAsync())
                            .ThrowsAsync(new TooManyRequestsException("Rate limit exceeded"));

                        services.AddScoped(_ => mockJokeService.Object);
                    });
                });

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/jokes/random");

            // Assert
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
            Assert.Equal("Rate limit exceeded", error!.Error);
        }

        [Fact]
        public async Task SearchJokes_WhenValidationFails_Returns400()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();
            var searchRequest = new SearchRequestDTO { Term = "", Limit = 0 };

            // Act
            var response = await client.PostAsync("/api/jokes/search", 
                new StringContent(JsonSerializer.Serialize(searchRequest), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SearchJokes_WhenApiTimeoutExceptionThrown_Returns504()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var mockSearchService = new Mock<IJokeSearchService>();
                        mockSearchService.Setup(s => s.SearchJokesAsync(It.IsAny<SearchRequestDTO>()))
                            .ThrowsAsync(new ApiTimeoutException("API request timed out"));

                        services.AddScoped(_ => mockSearchService.Object);
                    });
                });

            var client = factory.CreateClient();
            var searchRequest = new SearchRequestDTO { Term = "test", Limit = 5 };

            // Act
            var response = await client.PostAsync("/api/jokes/search", 
                new StringContent(JsonSerializer.Serialize(searchRequest), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
            Assert.Equal("API request timed out", error!.Error);
        }

        private class ErrorResponse
        {
            public string? Error { get; set; }
        }
    }
}
