using FilmRatingService.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FilmRatingService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _apiKey = "b2115f4c7e35c28847af5b084345ef7d";
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
        }

        public async Task<IActionResult> Index()
        {
            int featuredMovieId = 157336;
            MovieDetails featuredMovie = await GetMovieDetails(featuredMovieId);

            MovieListResponse popularMoviesResponse = await GetPopularMovies();

            FeaturedMovieViewModel viewModel = new FeaturedMovieViewModel
            {
                Title = featuredMovie?.Title ?? "Featured Movie Title",
                Description = featuredMovie?.Overview ?? "Featured Movie Description",
                CoverImageUrl = featuredMovie?.PosterPath != null ? $"https://image.tmdb.org/t/p/w500/{featuredMovie.PosterPath}" : "/images/default-poster.png",
                VideoUrl = "#",
                Rating = featuredMovie?.VoteAverage ?? 7.0,
                Likes = 0,
                Hearts = 0,
                PopularMovies = popularMoviesResponse?.Results ?? new List<MovieDetails>()
            };

            return View(viewModel);
        }

        private async Task<MovieDetails> GetMovieDetails(int movieId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<MovieDetails>($"movie/{movieId}?api_key={_apiKey}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching movie details for ID {movieId}: {ex.Message}");
                return null;
            }
        }

        private async Task<MovieListResponse> GetPopularMovies()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<MovieListResponse>($"movie/popular?api_key={_apiKey}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching popular movies: {ex.Message}");
                return null;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class MovieDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }
    }

    public class MovieListResponse
    {
        [JsonPropertyName("results")]
        public List<MovieDetails> Results { get; set; }
    }
}