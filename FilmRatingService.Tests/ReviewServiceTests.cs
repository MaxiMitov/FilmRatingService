using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Required for DbContextOptions and InMemory
using FilmRatingService.Services;     // Your ReviewService namespace
using FilmRatingService.Interfaces;   // Your IReviewService namespace
using FilmRatingService.Models;       // Your Models namespace (UserReview)
using FilmRatingService.Areas.Identity.Data; // Your ApplicationDbContext and ApplicationUser namespace
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FilmRatingService.Tests
{
    [TestFixture]
    public class ReviewServiceTests
    {
        private ApplicationDbContext _context; // Use a real (in-memory) DbContext instance
        private Mock<ILogger<ReviewService>> _mockLogger;
        private ReviewService _reviewService; // The service we are testing

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<ReviewService>>();
            _reviewService = new ReviewService(_context, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted(); // Ensures DB is deleted after each test for better isolation
            _context.Dispose();
        }

        [Test]
        public async Task AddReviewAsync_WithValidReview_AddsReviewToDatabase()
        {
            // Arrange
            var newReview = new UserReview
            {
                MovieId = 100,
                UserId = "test-user-id-123",
                Rating = 8,
                ReviewText = "Great movie!",
            };

            // Act
            await _reviewService.AddReviewAsync(newReview);

            // Assert
            Assert.That(_context.UserReviews.Count(), Is.EqualTo(1));
            var addedReview = await _context.UserReviews.FirstAsync();
            Assert.That(addedReview.MovieId, Is.EqualTo(newReview.MovieId));
            Assert.That(addedReview.UserId, Is.EqualTo(newReview.UserId));
            Assert.That(addedReview.Rating, Is.EqualTo(newReview.Rating));
            Assert.That(addedReview.ReviewText, Is.EqualTo(newReview.ReviewText));
            Assert.That(addedReview.Id, Is.GreaterThan(0));
            Assert.That(addedReview.ReviewDate, Is.Not.EqualTo(default(DateTime)));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully added review")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        [Test]
        public async Task GetReviewsForMovieAsync_WhenReviewsExist_ReturnsReviewsWithUserDetails()
        {
            // Arrange
            var movieIdWithReviews = 101;
            var testUser = new ApplicationUser { Id = "user-for-review-1", UserName = "reviewer@example.com", Name = "Test Reviewer" };
            _context.Users.Add(testUser);

            var review1 = new UserReview { MovieId = movieIdWithReviews, UserId = testUser.Id, User = testUser, Rating = 8, ReviewText = "Review 1 for movie 101", ReviewDate = DateTime.UtcNow.AddDays(-1) };
            var review2 = new UserReview { MovieId = movieIdWithReviews, UserId = testUser.Id, User = testUser, Rating = 9, ReviewText = "Review 2 for movie 101", ReviewDate = DateTime.UtcNow };
            var reviewForAnotherMovie = new UserReview { MovieId = 202, UserId = testUser.Id, User = testUser, Rating = 7, ReviewText = "Review for movie 202", ReviewDate = DateTime.UtcNow };

            _context.UserReviews.AddRange(review1, review2, reviewForAnotherMovie);
            await _context.SaveChangesAsync();

            // Act
            var result = (await _reviewService.GetReviewsForMovieAsync(movieIdWithReviews)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2), "Should only return reviews for the specified movie ID.");

            Assert.That(result[0].ReviewText, Is.EqualTo("Review 2 for movie 101"), "Reviews should be ordered by date descending (newest first).");
            Assert.That(result[1].ReviewText, Is.EqualTo("Review 1 for movie 101"));

            Assert.That(result[0].User, Is.Not.Null, "User details should be included.");
            Assert.That(result[0].User.UserName, Is.EqualTo(testUser.UserName));
            Assert.That(result[0].User.Name, Is.EqualTo(testUser.Name));
        }

        [Test]
        public async Task GetReviewsForMovieAsync_WhenNoReviewsExist_ReturnsEmptyList()
        {
            // Arrange
            var movieIdWithNoReviews = 303;

            // Act
            var result = await _reviewService.GetReviewsForMovieAsync(movieIdWithNoReviews);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0), "Should return an empty list if no reviews exist for the movie ID.");
        }

        [Test]
        public async Task GetReviewsForMovieAsync_WithInvalidMovieId_ReturnsEmptyListAndLogsWarning()
        {
            // Arrange
            var invalidMovieId = 0;

            // Act
            var result = await _reviewService.GetReviewsForMovieAsync(invalidMovieId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0), "Should return an empty list for an invalid movie ID.");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"GetReviewsForMovieAsync called with invalid MovieId: {invalidMovieId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        // <<< NEW TESTS FOR GetAllReviewsAsync, GetReviewByIdAsync, DeleteReviewAsync ADDED BELOW >>>

        // --- Tests for GetAllReviewsAsync ---
        [Test]
        public async Task GetAllReviewsAsync_WhenReviewsExist_ReturnsAllReviewsWithUserDetails()
        {
            // Arrange
            var user1 = new ApplicationUser { Id = "user1-ga", UserName = "user1ga@example.com", Name = "User One GA" };
            var user2 = new ApplicationUser { Id = "user2-ga", UserName = "user2ga@example.com", Name = "User Two GA" };
            _context.Users.AddRange(user1, user2);

            var review1 = new UserReview { MovieId = 101, UserId = user1.Id, User = user1, Rating = 8, ReviewText = "Review A", ReviewDate = DateTime.UtcNow.AddHours(-2) };
            var review2 = new UserReview { MovieId = 102, UserId = user2.Id, User = user2, Rating = 9, ReviewText = "Review B", ReviewDate = DateTime.UtcNow.AddHours(-1) };
            var review3 = new UserReview { MovieId = 103, UserId = user1.Id, User = user1, Rating = 7, ReviewText = "Review C", ReviewDate = DateTime.UtcNow };

            _context.UserReviews.AddRange(review1, review2, review3);
            await _context.SaveChangesAsync();

            // Act
            var result = (await _reviewService.GetAllReviewsAsync()).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].ReviewText, Is.EqualTo("Review C")); // Newest first
            Assert.That(result[1].ReviewText, Is.EqualTo("Review B"));
            Assert.That(result[2].ReviewText, Is.EqualTo("Review A"));
            Assert.That(result[0].User, Is.Not.Null);
            Assert.That(result[0].User.Name, Is.EqualTo(user1.Name));
        }

        [Test]
        public async Task GetAllReviewsAsync_WhenNoReviewsExist_ReturnsEmptyList()
        {
            // Arrange
            // Act
            var result = await _reviewService.GetAllReviewsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        // --- Tests for GetReviewByIdAsync ---
        [Test]
        public async Task GetReviewByIdAsync_WhenReviewExists_ReturnsReviewWithUserDetails()
        {
            // Arrange
            var testUser = new ApplicationUser { Id = "user-gbi", UserName = "gbi@example.com", Name = "User GetById" };
            _context.Users.Add(testUser);

            var seededReview = new UserReview { MovieId = 707, UserId = testUser.Id, User = testUser, Rating = 9, ReviewText = "Specific Review Text", ReviewDate = DateTime.UtcNow };
            _context.UserReviews.Add(seededReview);
            await _context.SaveChangesAsync();

            // Act
            var result = await _reviewService.GetReviewByIdAsync(seededReview.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(seededReview.Id));
            Assert.That(result.ReviewText, Is.EqualTo("Specific Review Text"));
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.Name, Is.EqualTo(testUser.Name));
        }

        [Test]
        public async Task GetReviewByIdAsync_WhenReviewDoesNotExist_ReturnsNull()
        {
            // Arrange
            var nonExistentReviewId = 9999;

            // Act
            var result = await _reviewService.GetReviewByIdAsync(nonExistentReviewId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetReviewByIdAsync_WithInvalidId_ReturnsNullAndLogsWarning()
        {
            // Arrange
            var invalidReviewId = 0;

            // Act
            var result = await _reviewService.GetReviewByIdAsync(invalidReviewId);

            // Assert
            Assert.That(result, Is.Null);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"GetReviewByIdAsync called with invalid reviewId: {invalidReviewId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        // --- Tests for DeleteReviewAsync ---
        [Test]
        public async Task DeleteReviewAsync_WhenReviewExists_DeletesReviewAndReturnsTrue()
        {
            // Arrange
            var reviewToDelete = new UserReview { MovieId = 808, UserId = "user-to-delete", Rating = 5, ReviewText = "To be deleted", ReviewDate = DateTime.UtcNow };
            _context.UserReviews.Add(reviewToDelete);
            await _context.SaveChangesAsync();
            var reviewIdToDelete = reviewToDelete.Id; // Get ID after saving

            Assert.That(_context.UserReviews.Count(), Is.EqualTo(1));

            // Act
            var result = await _reviewService.DeleteReviewAsync(reviewIdToDelete);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_context.UserReviews.Count(), Is.EqualTo(0), "Review should have been removed from the database.");
            var deletedReview = await _context.UserReviews.FindAsync(reviewIdToDelete);
            Assert.That(deletedReview, Is.Null, "FindAsync should return null for deleted review.");
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Successfully deleted review with Id: {reviewIdToDelete}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        [Test]
        public async Task DeleteReviewAsync_WhenReviewDoesNotExist_ReturnsFalseAndLogsWarning()
        {
            // Arrange
            var nonExistentReviewId = 9876;

            // Act
            var result = await _reviewService.DeleteReviewAsync(nonExistentReviewId);

            // Assert
            Assert.That(result, Is.False);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Review with Id: {nonExistentReviewId} not found for deletion.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        [Test]
        public async Task DeleteReviewAsync_WithInvalidId_ReturnsFalseAndLogsWarning()
        {
            // Arrange
            var invalidReviewId = 0;

            // Act
            var result = await _reviewService.DeleteReviewAsync(invalidReviewId);

            // Assert
            Assert.That(result, Is.False);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"DeleteReviewAsync called with invalid reviewId: {invalidReviewId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }
    }
}