using FilmRatingService.Interfaces;
using FilmRatingService.Models;

using Microsoft.AspNetCore.Mvc; // ResponseCacheAttribute is in this namespace
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

        // MODIFIED: Added [ResponseCache] attribute to the Index action
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page" })]
        // Duration is in seconds (e.g., 600 seconds = 10 minutes)
        // Location = ResponseCacheLocation.Any allows caching by client and proxies
        // VaryByQueryKeys ensures that different pages of results are cached separately
        public async Task<IActionResult> Index([FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            int featuredMovieId = 157336;
            MovieDetails featuredMovie = await _movieService.GetMovieDetailsAsync(featuredMovieId);
            MovieListResponse popularMoviesResponse = await _movieService.GetPopularMoviesAsync(page);

            var viewModel = new FeaturedMovieViewModel // Using FeaturedMovieViewModel
            {
                Title = featuredMovie?.Title ?? "Featured Movie Title",
                Description = featuredMovie?.Overview ?? "Featured Movie Description",
                CoverImageUrl = featuredMovie?.PosterPath != null ? $"https://image.tmdb.org/t/p/w500/{featuredMovie.PosterPath}" : "/images/default-poster.png",
                VideoUrl = "#",
                Rating = featuredMovie?.VoteAverage ?? 7.0,
                Likes = 0,
                Hearts = 0,
                PopularMovies = popularMoviesResponse?.Results ?? new List<MovieDetails>(),
                PopularMoviesCurrentPage = popularMoviesResponse?.Page ?? page,
                PopularMoviesTotalPages = popularMoviesResponse?.TotalPages ?? 0
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // This Error action is typically for unhandled exceptions (used by UseExceptionHandler)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // This disables caching for the error page
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // Using ErrorViewModel
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var searchViewModel = new MovieSearchViewModel { Query = query, CurrentPage = page };

            if (string.IsNullOrWhiteSpace(query))
            {
                searchViewModel.Results = new List<MovieDetails>();
                searchViewModel.TotalPages = 0;
                searchViewModel.TotalResults = 0;
                return View("SearchResults", searchViewModel);
            }

            MovieListResponse response = await _movieService.SearchMoviesAsync(query, page);

            searchViewModel.Results = response?.Results ?? new List<MovieDetails>();
            searchViewModel.CurrentPage = response?.Page ?? page;
            searchViewModel.TotalPages = response?.TotalPages ?? 0;
            searchViewModel.TotalResults = response?.TotalResults ?? 0;

            return View("SearchResults", searchViewModel);
        }

        [Route("Home/HttpStatusCodeHandler/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 401:
                    ViewData["ErrorMessage"] = "You are not authorized to access this resource. Please log in.";
                    return View("Unauthorized");
                case 403:
                    ViewData["ErrorMessage"] = "You do not have permission to access this resource.";
                    return View("Forbidden");
                case 404:
                    ViewData["ErrorMessage"] = "Sorry, the page you requested could not be found.";
                    return View("NotFound");
                default:
                    ViewData["StatusCode"] = statusCode;
                    ViewData["ErrorMessage"] = $"An unexpected error occurred (Status Code: {statusCode}). Please try again later or contact support.";
                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}