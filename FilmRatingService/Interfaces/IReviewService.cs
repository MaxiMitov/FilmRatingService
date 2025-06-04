using FilmRatingService.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmRatingService.Interfaces
{
    public interface IReviewService
    {
        Task AddReviewAsync(UserReview review);
        // MODIFIED: Added sortBy parameter
        Task<IEnumerable<UserReview>> GetReviewsForMovieAsync(int movieId, string sortBy = "date_desc");
        Task<IEnumerable<UserReview>> GetAllReviewsAsync(); // We can add sorting here later if needed
        Task<UserReview> GetReviewByIdAsync(int reviewId);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}