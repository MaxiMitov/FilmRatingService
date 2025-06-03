using FilmRatingService.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmRatingService.Interfaces
{
    public interface IReviewService
    {
        Task AddReviewAsync(UserReview review);
        Task<IEnumerable<UserReview>> GetReviewsForMovieAsync(int movieId);
        Task<IEnumerable<UserReview>> GetAllReviewsAsync();
        Task<UserReview> GetReviewByIdAsync(int reviewId); // <<< ADD THIS
        Task<bool> DeleteReviewAsync(int reviewId);      // <<< ADD THIS
    }
}