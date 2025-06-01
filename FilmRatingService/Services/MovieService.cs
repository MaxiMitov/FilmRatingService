using FilmRatingService.Interfaces;
using FilmRatingService.Models; // <<< ADD THIS LINE
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
        // ... rest of the code remains the same
        private readonly HttpClient _httpClient;
        private readonly ILogger<MovieService> _logger;
        private readonly string _apiKey;

        public MovieService(IHttpClientFactory httpClientFactory, ILogger<MovieService> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("TMDB");
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            }
            _logger = logger;
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

        public async Task<MovieListResponse> GetPopularMoviesAsync()
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

        public async Task<MovieListResponse> SearchMoviesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new MovieListResponse { Results = new List<MovieDetails>() };
            }
            try
            {
                // Using TMDB's actual search endpoint is better:
                var searchResponse = await _httpClient.GetFromJsonAsync<MovieListResponse>($"search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}");
                return searchResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error searching movies with query '{query}': {ex.Message}");
                return new MovieListResponse { Results = new List<MovieDetails>() };
            }
        }
    }
}