using FilmRatingService.Interfaces; // Required for IMovieService
using FilmRatingService.Models;   // Required for MovieDetails, MovieListResponse, FeaturedMovieViewModel, ErrorViewModel

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Collections.Generic; // Required for List<>
using System.Diagnostics;
using System.Threading.Tasks;

namespace FilmRatingService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMovieService _movieService; // Use the service interface

        // Constructor updated to inject IMovieService instead of IHttpClientFactory
        public HomeController(ILogger<HomeController> logger, IMovieService movieService)
        {
            _logger = logger;
            _movieService = movieService;
        }

        public async Task<IActionResult> Index()
        {
            int featuredMovieId = 157336; // Interstellar

            // Use the movie service to get data
            MovieDetails featuredMovie = await _movieService.GetMovieDetailsAsync(featuredMovieId);
            MovieListResponse popularMoviesResponse = await _movieService.GetPopularMoviesAsync();

            // Ensure your FeaturedMovieViewModel is correctly defined, possibly in the Models folder
            var viewModel = new FeaturedMovieViewModel
            {
                Title = featuredMovie?.Title ?? "Featured Movie Title",
                Description = featuredMovie?.Overview ?? "Featured Movie Description",
                CoverImageUrl = featuredMovie?.PosterPath != null ? $"https://image.tmdb.org/t/p/w500/{featuredMovie.PosterPath}" : "/images/default-poster.png",
                VideoUrl = "#", // Placeholder, you might want to fetch actual video/trailer URLs via the service
                Rating = featuredMovie?.VoteAverage ?? 7.0,
                Likes = 0, // Placeholder
                Hearts = 0, // Placeholder
                PopularMovies = popularMoviesResponse?.Results ?? new List<MovieDetails>()
            };

            return View(viewModel);
        }

        // The private GetMovieDetails and GetPopularMovies methods are removed from here.
        // Their logic is now in MovieService.cs

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Ensure ErrorViewModel is correctly defined, possibly in the Models folder
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Return an empty list to the view if the query is empty
                return View("SearchResults", new List<MovieDetails>());
            }

            // Use the movie service for searching
            MovieListResponse response = await _movieService.SearchMoviesAsync(query);
            var results = response?.Results ?? new List<MovieDetails>();

            return View("SearchResults", results);
        }
    }

    // IMPORTANT:
    // DELETE the class definitions for MovieDetails and MovieListResponse from this file.
    // They should now be in their own files within the FilmRatingService/Models/ folder,
    // with the namespace FilmRatingService.Models.
    //
    // Example (these should NOT be here anymore):
    // public class MovieDetails { ... } // <-- DELETE THIS FROM HERE
    // public class MovieListResponse { ... } // <-- DELETE THIS FROM HERE
}