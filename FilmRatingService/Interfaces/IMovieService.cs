using FilmRatingService.Models; // <<< ADD THIS LINE

using System.Threading.Tasks;

namespace FilmRatingService.Interfaces
{
    public interface IMovieService
    {
        Task<MovieDetails> GetMovieDetailsAsync(int movieId);
        Task<MovieListResponse> GetPopularMoviesAsync();
        Task<MovieListResponse> SearchMoviesAsync(string query);
    }
}