using System.Collections.Generic; // For List<>
using System.Text.Json.Serialization; // Keep this if you use System.Text.Json attributes

namespace FilmRatingService.Models // Ensure this namespace
{
    public class MovieListResponse
    {
        [JsonPropertyName("page")] // Example: TMDB often includes page info
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<MovieDetails> Results { get; set; }

        [JsonPropertyName("total_pages")] // Example: TMDB often includes page info
        public int TotalPages { get; set; }

        [JsonPropertyName("total_results")] // Example: TMDB often includes page info
        public int TotalResults { get; set; }
    }
}