using FilmRatingService.Interfaces;
using FilmRatingService.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FilmRatingService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMovieService _movieService;

        public HomeController(ILogger<HomeController> logger, IMovieService movieService)
        {
            _logger = logger;
            _movieService = movieService;
        }

        // MODIFIED Index action to accept a page number
        public async Task<IActionResult> Index([FromQuery] int page = 1) // Use [FromQuery] to make it clear it comes from querystring
        {
            if (page < 1) page = 1; // Ensure page is not less than 1

            int featuredMovieId = 157336; // Interstellar - consider making this dynamic or configurable

            MovieDetails featuredMovie = await _movieService.GetMovieDetailsAsync(featuredMovieId);

            // Call the service with the current page number
            MovieListResponse popularMoviesResponse = await _movieService.GetPopularMoviesAsync(page);

            var viewModel = new FeaturedMovieViewModel // Using the updated FeaturedMovieViewModel
            {
                Title = featuredMovie?.Title ?? "Featured Movie Title",
                Description = featuredMovie?.Overview ?? "Featured Movie Description",
                CoverImageUrl = featuredMovie?.PosterPath != null ? $"https://image.tmdb.org/t/p/w500/{featuredMovie.PosterPath}" : "/images/default-poster.png",
                VideoUrl = "#",
                Rating = featuredMovie?.VoteAverage ?? 7.0,
                Likes = 0,
                Hearts = 0,

                PopularMovies = popularMoviesResponse?.Results ?? new List<MovieDetails>(),
                // Populate paging properties
                PopularMoviesCurrentPage = popularMoviesResponse?.Page ?? page,
                PopularMoviesTotalPages = popularMoviesResponse?.TotalPages ?? 0
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // Using ErrorViewModel
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query) // Consider adding paging to search results later
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View("SearchResults", new List<MovieDetails>());
            }
            MovieListResponse response = await _movieService.SearchMoviesAsync(query);
            var results = response?.Results ?? new List<MovieDetails>();
            return View("SearchResults", results);
        }
    }
}