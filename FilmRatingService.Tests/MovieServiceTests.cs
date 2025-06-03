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
using System;                 // Required for Func
using System.Collections.Generic; // Required for List

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
                Content = new StringContent(apiResponseJson, System.Text.Encoding.UTF8, "application/json")
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

        [Test]
        public async Task GetMovieDetailsAsync_WhenApiCallFails_ReturnsNull()
        {
            // Arrange
            var movieId = 999;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"movie/{movieId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _movieService.GetMovieDetailsAsync(movieId);

            // Assert
            Assert.That(result, Is.Null, "Service should return null when API call fails or movie is not found.");
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error fetching movie details for ID {movieId}")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        [Test]
        public async Task GetPopularMoviesAsync_WhenApiCallIsSuccessful_ReturnsMovieListResponse()
        {
            // Arrange
            var pageNumber = 1;
            var expectedResponse = new MovieListResponse
            {
                Page = pageNumber,
                Results = new List<MovieDetails>
                {
                    new MovieDetails { Id = 1, Title = "Popular Movie 1" },
                    new MovieDetails { Id = 2, Title = "Popular Movie 2" }
                },
                TotalPages = 5,
                TotalResults = 100
            };
            var apiResponseJson = JsonSerializer.Serialize(expectedResponse);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(apiResponseJson, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"movie/popular?api_key=test_api_key&page={pageNumber}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage)
                .Verifiable(); // Mark as verifiable

            // Act
            var result = await _movieService.GetPopularMoviesAsync(pageNumber);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Page, Is.EqualTo(expectedResponse.Page));
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Count, Is.EqualTo(expectedResponse.Results.Count));
            if (result.Results.Any())
            {
                Assert.That(result.Results[0].Title, Is.EqualTo(expectedResponse.Results[0].Title));
            }
            Assert.That(result.TotalPages, Is.EqualTo(expectedResponse.TotalPages));

            _mockHttpMessageHandler.Verify(); // <<< CORRECTED THIS LINE (line 196 in your error)
        }

        [Test]
        public async Task GetPopularMoviesAsync_WhenApiCallFails_ReturnsEmptyResponseWithCorrectPage()
        {
            // Arrange
            var pageNumber = 1;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("movie/popular")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _movieService.GetPopularMoviesAsync(pageNumber);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results, Is.Not.Null.And.Empty);
            Assert.That(result.Page, Is.EqualTo(pageNumber));
            Assert.That(result.TotalPages, Is.EqualTo(0));

            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error fetching popular movies for page {pageNumber}")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        [Test]
        public async Task SearchMoviesAsync_WithValidQueryAndPage_ReturnsMovieListResponse()
        {
            // Arrange
            var query = "TestQuery";
            var pageNumber = 2;
            var expectedResponse = new MovieListResponse
            {
                Page = pageNumber,
                Results = new List<MovieDetails> { new MovieDetails { Id = 101, Title = "Found Movie Alpha" } },
                TotalPages = 3,
                TotalResults = 30
            };
            var apiResponseJson = JsonSerializer.Serialize(expectedResponse);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(apiResponseJson, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"search/movie?api_key=test_api_key&query={query}&page={pageNumber}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage)
                .Verifiable();

            // Act
            var result = await _movieService.SearchMoviesAsync(query, pageNumber);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Page, Is.EqualTo(expectedResponse.Page));
            Assert.That(result.Results, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Results[0].Title, Is.EqualTo(expectedResponse.Results[0].Title));

            _mockHttpMessageHandler.Verify(); // <<< CORRECTED THIS LINE (line 281 in your error)
        }

        [Test]
        public async Task SearchMoviesAsync_WithEmptyQuery_ReturnsEmptyResponseAndDoesNotCallApi()
        {
            // Arrange
            var query = "";
            var pageNumber = 1;

            // Act
            var result = await _movieService.SearchMoviesAsync(query, pageNumber);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results, Is.Not.Null.And.Empty);
            Assert.That(result.Page, Is.EqualTo(1));
            Assert.That(result.TotalPages, Is.EqualTo(0));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Test]
        public async Task SearchMoviesAsync_WhenApiCallFails_ReturnsEmptyResponseWithCorrectPage()
        {
            // Arrange
            var query = "ValidQuery";
            var pageNumber = 1;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"search/movie?api_key=test_api_key&query={query}&page={pageNumber}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _movieService.SearchMoviesAsync(query, pageNumber);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results, Is.Not.Null.And.Empty);
            Assert.That(result.Page, Is.EqualTo(pageNumber));
            Assert.That(result.TotalPages, Is.EqualTo(0));

            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error searching movies with query '{query}' for page {pageNumber}")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }
    }
}