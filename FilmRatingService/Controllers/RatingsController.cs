using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FilmRatingService.Interfaces; // <<< ADD THIS for IReviewService
using System.Linq;                // <<< ADD THIS for .Any(), .Average(), .Count()
using System.Threading.Tasks;     // <<< ADD THIS for Task
using System.Collections.Generic; // Keep this if any method still uses it, though GetMovieRatings won't directly return List<object> anymore

namespace FilmRatingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly ILogger<RatingsController> _logger;
        private readonly IReviewService _reviewService; // <<< ADD IReviewService field

        // MODIFIED CONSTRUCTOR to inject IReviewService
        public RatingsController(ILogger<RatingsController> logger, IReviewService reviewService)
        {
            _logger = logger;
            _reviewService = reviewService; // <<< ASSIGN IReviewService
        }

        // MODIFIED GetMovieRatings to be async and use IReviewService
        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetMovieRatings(int movieId)
        {
            _logger.LogInformation("API: Request received for real ratings for movieId: {MovieId}", movieId);

            if (movieId <= 0)
            {
                _logger.LogWarning("API: Invalid movie ID ({MovieId}) received.", movieId);
                return BadRequest("Invalid movie ID.");
            }

            var reviews = await _reviewService.GetReviewsForMovieAsync(movieId);

            if (reviews == null)
            {
                // This case should ideally not be hit if your ReviewService.GetReviewsForMovieAsync
                // returns an empty list on error or no data, rather than null.
                // If it can return null, this check is fine.
                _logger.LogError("API: Review service returned null for movieId: {MovieId}. This might indicate an issue in the service.", movieId);
                return StatusCode(500, "An error occurred while fetching reviews. Please try again later.");
            }

            double averageRating = 0;
            int totalReviewsCount = 0;

            if (reviews.Any()) // Check if there are any reviews to avoid division by zero or errors on empty list
            {
                averageRating = reviews.Average(r => r.Rating);
                totalReviewsCount = reviews.Count();
            }

            var ratingSummary = new
            {
                MovieId = movieId,
                AverageRating = averageRating, // This is now calculated
                TotalReviews = totalReviewsCount  // This is now calculated
                // UserRating is removed as it's not directly calculated here from all reviews;
                // it would typically be specific to the logged-in user if we were fetching that.
            };

            _logger.LogInformation("API: Returning rating summary for movieId: {MovieId} - AvgRating: {AverageRating:F2}, Count: {TotalReviews}",
                                   movieId, ratingSummary.AverageRating, ratingSummary.TotalReviews);

            return Ok(ratingSummary);
        }

        // Your placeholder POST method and UserRatingInputModel class can remain as they were,
        // commented out or not, based on your preference for future use.
        // // Example of another placeholder method you might add later:
        // // [HttpPost("{movieId}/rate")]
        // // public IActionResult SubmitRating(int movieId, [FromBody] UserRatingInputModel ratingInput)
        // // { ... }
        //
        // // public class UserRatingInputModel { ... }
    }
}