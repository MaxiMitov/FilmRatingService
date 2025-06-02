using FilmRatingService.Models; // For UserReview

using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmRatingService.Interfaces
{
    public interface IReviewService
    {
        Task AddReviewAsync(UserReview review);
        Task<IEnumerable<UserReview>> GetReviewsForMovieAsync(int movieId);
        // We can add methods for Update, Delete, GetById later
    }
}