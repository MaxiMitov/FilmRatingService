using FilmRatingService.Areas.Identity.Data; // For ApplicationDbContext
using FilmRatingService.Interfaces;
using FilmRatingService.Models; // For UserReview

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmRatingService.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddReviewAsync(UserReview review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            try
            {
                // Ensure ReviewDate is set if not already
                if (review.ReviewDate == default)
                {
                    review.ReviewDate = DateTime.UtcNow;
                }

                _context.UserReviews.Add(review);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added review for MovieId: {MovieId} by UserId: {UserId}", review.MovieId, review.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for MovieId: {MovieId} by UserId: {UserId}", review.MovieId, review.UserId);
                throw; // Re-throw the exception to be handled by the caller
            }
        }

        public async Task<IEnumerable<UserReview>> GetReviewsForMovieAsync(int movieId)
        {
            if (movieId <= 0)
            {
                _logger.LogWarning("GetReviewsForMovieAsync called with invalid MovieId: {MovieId}", movieId);
                return new List<UserReview>(); // Return empty list for invalid ID
            }

            try
            {
                // Retrieve reviews and include the User navigation property to get user details (like name)
                // Order by date, newest first
                return await _context.UserReviews
                                     .Include(r => r.User) // Eagerly load the related ApplicationUser
                                     .Where(r => r.MovieId == movieId)
                                     .OrderByDescending(r => r.ReviewDate)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reviews for MovieId: {MovieId}", movieId);
                return new List<UserReview>(); // Return empty list on error
            }
        }
    }
}