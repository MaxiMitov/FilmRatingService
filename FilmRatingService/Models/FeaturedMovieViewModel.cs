﻿using System.Collections.Generic;

using FilmRatingService.Models; // Required if MovieDetails is in this namespace, which it should be

namespace FilmRatingService.Models
{
    public class FeaturedMovieViewModel
    {
        public int Id { get; set; } // <<< ADD THIS PROPERTY for the featured movie's ID
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public double Rating { get; set; }
        public int Likes { get; set; } // These seem like placeholders for now
        public int Hearts { get; set; } // These seem like placeholders for now

        // Popular Movies Section
        public List<MovieDetails> PopularMovies { get; set; }

        // Paging Properties for Popular Movies
        public int PopularMoviesCurrentPage { get; set; }
        public int PopularMoviesTotalPages { get; set; }

        public bool HasPreviousPopularMoviesPage => PopularMoviesCurrentPage > 1;
        public bool HasNextPopularMoviesPage => PopularMoviesCurrentPage < PopularMoviesTotalPages;
    }
}