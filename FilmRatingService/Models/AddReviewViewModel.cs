using System.ComponentModel.DataAnnotations;

namespace FilmRatingService.Models
{
    public class AddReviewViewModel
    {
        public int MovieId { get; set; } // Hidden field in the form, to know which movie is being reviewed
        public string MovieTitle { get; set; } // To display on the review page

        [Required]
        [Range(1, 10, ErrorMessage = "Please select a rating between 1 and 10.")]
        public int Rating { get; set; } // User's rating (e.g., 1-10)

        [Display(Name = "Your Review")]
        [DataType(DataType.MultilineText)]
        [StringLength(5000, ErrorMessage = "Review text cannot exceed 5000 characters.")]
        public string ReviewText { get; set; }
    }
}