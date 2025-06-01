using FilmRatingService.Interfaces;
using FilmRatingService.Models; // Ensure MovieSearchViewModel is in this namespace

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // Using ErrorViewModel
        }

        // MODIFIED Search action
        [HttpGet]
        public async Task<IActionResult> Search(string query, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var searchViewModel = new MovieSearchViewModel { Query = query, CurrentPage = page };

            if (string.IsNullOrWhiteSpace(query))
            {
                // Optionally, you could set a message in the ViewModel like "Please enter a search term."
                // For now, just return the view with empty results and defaults.
                searchViewModel.Results = new List<MovieDetails>();
                searchViewModel.TotalPages = 0;
                searchViewModel.TotalResults = 0;
                return View("SearchResults", searchViewModel);
            }

            MovieListResponse response = await _movieService.SearchMoviesAsync(query, page);

            searchViewModel.Results = response?.Results ?? new List<MovieDetails>();
            searchViewModel.CurrentPage = response?.Page ?? page; // Ensure CurrentPage is set from response
            searchViewModel.TotalPages = response?.TotalPages ?? 0;
            searchViewModel.TotalResults = response?.TotalResults ?? 0;

            return View("SearchResults", searchViewModel);
        }
    }
}