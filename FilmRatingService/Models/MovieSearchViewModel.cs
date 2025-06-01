using System.Collections.Generic;

namespace FilmRatingService.Models
{
    public class MovieSearchViewModel
    {
        public string Query { get; set; }
        public List<MovieDetails> Results { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public MovieSearchViewModel()
        {
            Results = new List<MovieDetails>();
        }
    }
}