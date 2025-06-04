using FilmRatingService.Areas.Identity.Data;
using FilmRatingService.Interfaces;
using FilmRatingService.Models;

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

        // ... AddReviewAsync and GetAllReviewsAsync methods remain the same for now ...
        public async Task AddReviewAsync(UserReview review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));
            try
            {
                if (review.ReviewDate == default) review.ReviewDate = DateTime.UtcNow;
                _context.UserReviews.Add(review);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added review for MovieId: {MovieId} by UserId: {UserId}", review.MovieId, review.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for MovieId: {MovieId} by UserId: {UserId}", review.MovieId, review.UserId);
                throw;
            }
        }

        public async Task<IEnumerable<UserReview>> GetAllReviewsAsync()
        {
            try
            {
                return await _context.UserReviews
                                     .Include(r => r.User)
                                     .OrderByDescending(r => r.ReviewDate)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all reviews.");
                return new List<UserReview>();
            }
        }


        // MODIFIED: GetReviewsForMovieAsync to include sorting
        public async Task<IEnumerable<UserReview>> GetReviewsForMovieAsync(int movieId, string sortBy = "date_desc")
        {
            if (movieId <= 0)
            {
                _logger.LogWarning("GetReviewsForMovieAsync called with invalid MovieId: {MovieId}", movieId);
                return new List<UserReview>();
            }

            try
            {
                var query = _context.UserReviews
                                    .Include(r => r.User) // Eagerly load the related ApplicationUser
                                    .Where(r => r.MovieId == movieId)
                                    .AsQueryable(); // Prepare for dynamic sorting

                // Apply sorting
                switch (sortBy?.ToLowerInvariant()) // Use null-conditional operator and ToLowerInvariant for robust comparison
                {
                    case "date_asc":
                        query = query.OrderBy(r => r.ReviewDate);
                        break;
                    case "rating_desc":
                        query = query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.ReviewDate);
                        break;
                    case "rating_asc":
                        query = query.OrderBy(r => r.Rating).ThenBy(r => r.ReviewDate);
                        break;
                    case "date_desc": // Default case
                    default:
                        query = query.OrderByDescending(r => r.ReviewDate);
                        break;
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reviews for MovieId: {MovieId} with sortBy: {SortBy}", movieId, sortBy);
                return new List<UserReview>();
            }
        }

        public async Task<UserReview> GetReviewByIdAsync(int reviewId)
        {
            if (reviewId <= 0)
            {
                _logger.LogWarning("GetReviewByIdAsync called with invalid reviewId: {ReviewId}", reviewId);
                return null;
            }
            try
            {
                return await _context.UserReviews
                                     .Include(r => r.User)
                                     .FirstOrDefaultAsync(r => r.Id == reviewId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching review by Id: {ReviewId}", reviewId);
                return null;
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            if (reviewId <= 0)
            {
                _logger.LogWarning("DeleteReviewAsync called with invalid reviewId: {ReviewId}", reviewId);
                return false;
            }
            try
            {
                var reviewToDelete = await _context.UserReviews.FindAsync(reviewId);
                if (reviewToDelete == null)
                {
                    _logger.LogWarning("Review with Id: {ReviewId} not found for deletion.", reviewId);
                    return false;
                }
                _context.UserReviews.Remove(reviewToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted review with Id: {ReviewId}", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review with Id: {ReviewId}", reviewId);
                return false;
            }
        }
    }
}