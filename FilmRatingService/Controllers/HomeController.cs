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

        // <<< NEW ACTION METHOD FOR MOVIE DETAILS PAGE >>>
        public async Task<IActionResult> Details(int id) // 'id' will be the movieId
        {
            if (id <= 0)
            {
                // Invalid movie ID, redirect to a Bad Request error page
                return RedirectToAction("HttpStatusCodeHandler", new { statusCode = 400 });
            }

            var movieDetails = await _movieService.GetMovieDetailsAsync(id);

            if (movieDetails == null)
            {
                // Movie not found by the service, redirect to a Not Found error page
                return RedirectToAction("HttpStatusCodeHandler", new { statusCode = 404 });
            }

            // Pass the MovieDetails object to the Details view
            return View(movieDetails); // This will look for Views/Home/Details.cshtml
        }
    }
}