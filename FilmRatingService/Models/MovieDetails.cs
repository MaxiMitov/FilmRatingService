using System.Text.Json.Serialization; // Keep this if you use System.Text.Json attributes

namespace FilmRatingService.Models // Ensure this namespace
{
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

        // Add any other properties that were part of your original MovieDetails class
        // For example, if you had release_date, genres, etc.
        // [JsonPropertyName("release_date")]
        // public string ReleaseDate { get; set; }
    }
}