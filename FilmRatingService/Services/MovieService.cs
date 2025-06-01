using FilmRatingService.Interfaces;
using FilmRatingService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace FilmRatingService.Services
{
    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger; // Using ILogger from LoggerFactory as per your existing file
        private readonly string _apiKey;

        public MovieService(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("TMDB");
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            }
            _logger = loggerFactory.CreateLogger("MovieService"); // Matches your existing pattern
            _apiKey = configuration["TMDBApiKey"] ?? "b2115f4c7e35c28847af5b084345ef7d";
        }

        public async Task<MovieDetails> GetMovieDetailsAsync(int movieId)
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

        public async Task<MovieListResponse> GetPopularMoviesAsync(int pageNumber = 1)
        {
            if (pageNumber < 1) pageNumber = 1;
            try
            {
                return await _httpClient.GetFromJsonAsync<MovieListResponse>($"movie/popular?api_key={_apiKey}&page={pageNumber}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching popular movies for page {pageNumber}: {ex.Message}");
                return new MovieListResponse { Results = new List<MovieDetails>(), Page = pageNumber, TotalPages = 0, TotalResults = 0 };
            }
        }

        // MODIFIED SearchMoviesAsync to accept pageNumber
        public async Task<MovieListResponse> SearchMoviesAsync(string query, int pageNumber = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Return an empty response with valid pagination defaults if query is empty
                return new MovieListResponse { Results = new List<MovieDetails>(), Page = 1, TotalPages = 0, TotalResults = 0 };
            }

            if (pageNumber < 1) pageNumber = 1; // Ensure page number is valid

            try
            {
                // Append the page number to the API request
                var searchResponse = await _httpClient.GetFromJsonAsync<MovieListResponse>(
                    $"search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&page={pageNumber}");
                return searchResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error searching movies with query '{query}' for page {pageNumber}: {ex.Message}");
                // Return an empty response with valid pagination defaults on error
                return new MovieListResponse { Results = new List<MovieDetails>(), Page = pageNumber, TotalPages = 0, TotalResults = 0 };
            }
        }
    }
}