using FilmRatingService.Interfaces;
using FilmRatingService.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
// using Microsoft.AspNetCore.Diagnostics; // Potentially needed if you want to inspect IStatusCodeReExecuteFeature

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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Using ErrorViewModel
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // MODIFIED Search action from previous commit
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

        // HttpStatusCodeHandler method with new cases for 401 and 403
        [Route("Home/HttpStatusCodeHandler/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            // var reExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            // _logger.LogWarning($"Error {statusCode} for {reExecuteFeature?.OriginalPath} {reExecuteFeature?.OriginalQueryString}");

            switch (statusCode)
            {
                // <<< ADDED/MODIFIED THESE CASES >>>
                case 401:
                    ViewData["ErrorMessage"] = "You are not authorized to access this resource. Please log in.";
                    // This will look for Views/Home/Unauthorized.cshtml or Views/Shared/Unauthorized.cshtml
                    return View("Unauthorized");
                case 403:
                    ViewData["ErrorMessage"] = "You do not have permission to access this resource.";
                    // This will look for Views/Home/Forbidden.cshtml or Views/Shared/Forbidden.cshtml
                    return View("Forbidden");
                // <<< END OF ADDED/MODIFIED CASES >>>
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