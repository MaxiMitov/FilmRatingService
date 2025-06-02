using NUnit.Framework;
using Moq;
using Moq.Protected; // Required for mocking HttpMessageHandler
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FilmRatingService.Services; // Your MovieService namespace
using FilmRatingService.Interfaces; // Your IMovieService namespace
using FilmRatingService.Models;   // Your Models namespace
using System.Text.Json;       // For serializing test data
using System; // Required for Func

namespace FilmRatingService.Tests
{
    [TestFixture]
    public class MovieServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<ILoggerFactory> _mockLoggerFactory;
        private Mock<ILogger> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private MovieService _movieService;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new System.Uri("https://api.themoviedb.org/3/")
            };

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _mockLogger = new Mock<ILogger>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["TMDBApiKey"]).Returns("test_api_key");

            _movieService = new MovieService(
                _mockHttpClientFactory.Object,
                _mockLoggerFactory.Object,
                _mockConfiguration.Object
            );
        }

        [Test]
        public async Task GetMovieDetailsAsync_WhenApiCallIsSuccessful_ReturnsMovieDetails()
        {
            // Arrange
            var movieId = 123;
            var expectedMovieDetails = new MovieDetails
            {
                Id = movieId,
                Title = "Test Movie",
                Overview = "Test Overview",
                VoteAverage = 7.5
            };
            var apiResponseJson = JsonSerializer.Serialize(expectedMovieDetails);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(apiResponseJson)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"movie/{movieId}?api_key=test_api_key")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _movieService.GetMovieDetailsAsync(movieId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedMovieDetails.Id));
            Assert.That(result.Title, Is.EqualTo(expectedMovieDetails.Title));
            Assert.That(result.Overview, Is.EqualTo(expectedMovieDetails.Overview));
            Assert.That(result.VoteAverage, Is.EqualTo(expectedMovieDetails.VoteAverage));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains($"movie/{movieId}")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        // <<< NEW TEST METHOD ADDED HERE >>>
        [Test]
        public async Task GetMovieDetailsAsync_WhenApiCallFails_ReturnsNull()
        {
            // Arrange
            var movieId = 999; // An ID that we'll simulate as not found or causing an error

            // Simulate an unsuccessful API response (e.g., NotFound)
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound // Could also be InternalServerError, etc.
                // No content needed, or an empty StringContent if the API returns a body on error
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"movie/{movieId}")), // Check only relevant part of URL
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _movieService.GetMovieDetailsAsync(movieId);

            // Assert
            Assert.That(result, Is.Null, "Service should return null when API call fails or movie is not found.");

            // Optional: Verify that an error was logged.
            // Your MovieService's GetMovieDetailsAsync logs an error in the catch block.
            // This verification ensures that your error handling path is being invoked.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error), // Check that it's an Error log
                    It.IsAny<EventId>(), // We don't care about the specific EventId
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error fetching movie details for ID {movieId}")), // Check if the log message contains the expected text
                    It.IsAny<HttpRequestException>(), // Expect an HttpRequestException to be logged
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), // We don't care about the formatter
                Times.Once // Ensure it was logged exactly once
            );
        }

        // We will add more tests here for:
        // - GetPopularMoviesAsync_WhenApiCallIsSuccessful_ReturnsMovieListResponse
        // - GetPopularMoviesAsync_WhenApiCallFails_ReturnsEmptyOrNull
        // - SearchMoviesAsync_WithValidQuery_ReturnsMovieListResponse
        // etc.
    }
}