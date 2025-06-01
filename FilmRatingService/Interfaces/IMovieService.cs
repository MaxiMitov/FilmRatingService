using FilmRatingService.Models;

using System.Threading.Tasks;

namespace FilmRatingService.Interfaces
{
    public interface IMovieService
    {
        Task<MovieDetails> GetMovieDetailsAsync(int movieId);
        Task<MovieListResponse> GetPopularMoviesAsync(int pageNumber = 1);
        // MODIFIED: Added pageNumber parameter to SearchMoviesAsync
        Task<MovieListResponse> SearchMoviesAsync(string query, int pageNumber = 1);
    }
}