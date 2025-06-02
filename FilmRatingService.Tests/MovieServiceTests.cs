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

namespace FilmRatingService.Tests
{
    [TestFixture]
    public class MovieServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<ILoggerFactory> _mockLoggerFactory;
        private Mock<ILogger> _mockLogger; // Using ILogger directly as per MovieService constructor
        private Mock<IConfiguration> _mockConfiguration;
        private MovieService _movieService; // The service we are testing

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new System.Uri("https://api.themoviedb.org/3/") // Match base address
            };

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Mock ILoggerFactory and ILogger
            // Your MovieService uses loggerFactory.CreateLogger("MovieService");
            _mockLogger = new Mock<ILogger>(); // Generic ILogger mock
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);


            // Mock IConfiguration for the API key
            _mockConfiguration = new Mock<IConfiguration>();
            // Mock the configuration value for TMDBApiKey
            // You can use a mock configuration section if your actual config is more complex
            _mockConfiguration.Setup(c => c["TMDBApiKey"]).Returns("test_api_key");

            // Initialize the MovieService with mocked dependencies
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

            // Setup the protected SendAsync method of HttpMessageHandler
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"movie/{movieId}?api_key=test_api_key")), // Check URL
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

            // Verify that SendAsync was called on the HttpMessageHandler
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(), // We expect it to be called exactly once
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains($"movie/{movieId}")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        // We will add more tests here:
        // - GetMovieDetailsAsync_WhenApiCallFails_ReturnsNull
        // - GetPopularMoviesAsync_WhenApiCallIsSuccessful_ReturnsMovieListResponse
        // - SearchMoviesAsync_WithValidQuery_ReturnsMovieListResponse
        // etc.
    }
}