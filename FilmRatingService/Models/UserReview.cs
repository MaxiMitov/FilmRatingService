using FilmRatingService.Areas.Identity.Data; // For ApplicationUser

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmRatingService.Models
{
    public class UserReview
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; } // This will store the TMDB Movie ID

        // We won't store full MovieDetails here to avoid data duplication.
        // We can fetch MovieDetails from TMDB API using MovieId when needed.
        // If you want to store movie title for quick display, you can add it:
        // public string MovieTitle { get; set; } 

        [Required]
        [Range(1, 10, ErrorMessage = "Rating must be between 1 and 10.")] // Example: 1-10 rating scale
        public int Rating { get; set; }

        [DataType(DataType.MultilineText)]
        public string ReviewText { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; }

        // Foreign Key for ApplicationUser
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Navigation property back to the user
    }
}