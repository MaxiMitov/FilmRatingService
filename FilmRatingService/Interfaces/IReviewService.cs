using FilmRatingService.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmRatingService.Interfaces
{
    public interface IReviewService
    {
        Task AddReviewAsync(UserReview review);
        Task<IEnumerable<UserReview>> GetReviewsForMovieAsync(int movieId);
        Task<IEnumerable<UserReview>> GetAllReviewsAsync(); // <<< ADD THIS NEW METHOD
        // We can add methods for Update, Delete, GetReviewById later
    }
}