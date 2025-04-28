using FilmRatingService.Controllers;

using System.Collections.Generic;

namespace FilmRatingService.Models
{
    public class FeaturedMovieViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public double Rating { get; set; }
        public int Likes { get; set; }
        public int Hearts { get; set; }
        public List<MovieDetails> PopularMovies { get; set; }
    }
}