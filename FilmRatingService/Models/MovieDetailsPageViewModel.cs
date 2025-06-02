using System.Collections.Generic;

namespace FilmRatingService.Models
{
    public class MovieDetailsPageViewModel
    {
        public MovieDetails Movie { get; set; }
        public IEnumerable<UserReview> Reviews { get; set; }
        // public AddReviewViewModel NewReview { get; set; } // Optionally, can include a blank review form model

        public MovieDetailsPageViewModel()
        {
            Reviews = new List<UserReview>();
            // NewReview = new AddReviewViewModel();
        }
    }
}