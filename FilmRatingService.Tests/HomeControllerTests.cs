using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using FilmRatingService.Controllers;
using FilmRatingService.Interfaces;
using FilmRatingService.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System; // Required for IDisposable if TearDown is added

namespace FilmRatingService.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IMovieService> _mockMovieService;
        private Mock<IReviewService> _mockReviewService;
        private Mock<ILogger<HomeController>> _mockLogger;
        private HomeController _homeController;

        [SetUp]
        public void Setup()
        {
            _mockMovieService = new Mock<IMovieService>();
            _mockReviewService = new Mock<IReviewService>();
            _mockLogger = new Mock<ILogger<HomeController>>();

            _homeController = new HomeController(
                _mockLogger.Object,
                _mockMovieService.Object,
                _mockReviewService.Object
            );
        }

        [TearDown] // Added to dispose the controller as it's IDisposable
        public void TearDown()
        {
            _homeController?.Dispose();
            _homeController = null;
            _mockMovieService = null;
            _mockReviewService = null;
            _mockLogger = null;
        }

        [Test]
        public async Task Index_WhenCalled_ReturnsViewResultWithFeaturedMovieViewModel()
        {
            // Arrange
            var pageNumber = 1;
            var featuredMovieId = 157336;

            var dummyFeaturedMovie = new MovieDetails { Id = featuredMovieId, Title = "Test Featured Movie", Overview = "Featured Overview", VoteAverage = 8.0 };
            var dummyPopularMoviesResponse = new MovieListResponse
            {
                Page = pageNumber,
                Results = new List<MovieDetails> { new MovieDetails { Id = 1, Title = "Popular 1" } },
                TotalPages = 5,
                TotalResults = 50
            };

            _mockMovieService.Setup(service => service.GetMovieDetailsAsync(It.Is<int>(id => id == featuredMovieId)))
                             .ReturnsAsync(dummyFeaturedMovie);
            // MODIFIED Setup for GetPopularMoviesAsync
            _mockMovieService.Setup(service => service.GetPopularMoviesAsync(It.Is<int>(p => p == pageNumber)))
                             .ReturnsAsync(dummyPopularMoviesResponse);

            // Act
            var result = await _homeController.Index(pageNumber);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<FeaturedMovieViewModel>());
            var model = viewResult.Model as FeaturedMovieViewModel;
            Assert.That(model.Id, Is.EqualTo(dummyFeaturedMovie.Id));
            Assert.That(model.Title, Is.EqualTo(dummyFeaturedMovie.Title));
            Assert.That(model.PopularMovies.Count, Is.EqualTo(dummyPopularMoviesResponse.Results.Count));
            Assert.That(model.PopularMoviesCurrentPage, Is.EqualTo(pageNumber));
            Assert.That(model.PopularMoviesTotalPages, Is.EqualTo(dummyPopularMoviesResponse.TotalPages));

            _mockMovieService.Verify(service => service.GetMovieDetailsAsync(featuredMovieId), Times.Once);
            _mockMovieService.Verify(service => service.GetPopularMoviesAsync(pageNumber), Times.Once);
        }

        [Test]
        public async Task Details_WithValidId_ReturnsViewResultWithMovieDetailsPageViewModel()
        {
            // Arrange
            var movieId = 1;
            var dummyMovieDetails = new MovieDetails { Id = movieId, Title = "Detailed Test Movie" };
            var dummyReviews = new List<UserReview>
            {
                new UserReview { Id = 1, MovieId = movieId, ReviewText = "Great!", Rating = 9 },
                new UserReview { Id = 2, MovieId = movieId, ReviewText = "Awesome!", Rating = 10 }
            };

            // MODIFIED Setup for GetMovieDetailsAsync (though likely not the cause of CS0854, being explicit is good)
            _mockMovieService.Setup(s => s.GetMovieDetailsAsync(It.Is<int>(id => id == movieId)))
                             .ReturnsAsync(dummyMovieDetails);
            // MODIFIED Setup for GetReviewsForMovieAsync
            // HomeController.Details calls GetReviewsForMovieAsync(id) - the 'sortBy' parameter uses its default "date_desc"
            _mockReviewService.Setup(s => s.GetReviewsForMovieAsync(It.Is<int>(id => id == movieId),
                                                                    It.Is<string>(sort => sort == "date_desc")))
                              .ReturnsAsync(dummyReviews);

            // Act
            var result = await _homeController.Details(movieId);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<MovieDetailsPageViewModel>());
            var model = viewResult.Model as MovieDetailsPageViewModel;
            Assert.That(model.Movie, Is.EqualTo(dummyMovieDetails));
            Assert.That(model.Reviews.Count(), Is.EqualTo(dummyReviews.Count));

            _mockMovieService.Verify(s => s.GetMovieDetailsAsync(movieId), Times.Once);
            _mockReviewService.Verify(s => s.GetReviewsForMovieAsync(movieId, "date_desc"), Times.Once);
        }

        [Test]
        public async Task Details_WithInvalidId_ReturnsRedirectToHttpStatusCodeHandler400()
        {
            // Arrange
            var invalidMovieId = 0;

            // Act
            var result = await _homeController.Details(invalidMovieId);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("HttpStatusCodeHandler"));
            Assert.That(redirectResult.RouteValues["statusCode"], Is.EqualTo(400));
        }

        [Test]
        public async Task Details_WhenMovieNotFound_ReturnsRedirectToHttpStatusCodeHandler404()
        {
            // Arrange
            var nonExistentMovieId = 999;
            // MODIFIED Setup for GetMovieDetailsAsync
            _mockMovieService.Setup(s => s.GetMovieDetailsAsync(It.Is<int>(id => id == nonExistentMovieId)))
                             .ReturnsAsync((MovieDetails)null);

            // Act
            var result = await _homeController.Details(nonExistentMovieId);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("HttpStatusCodeHandler"));
            Assert.That(redirectResult.RouteValues["statusCode"], Is.EqualTo(404));
        }

        // More tests for HomeController actions (Search, Privacy, Error, HttpStatusCodeHandler) will go here.
    }
}