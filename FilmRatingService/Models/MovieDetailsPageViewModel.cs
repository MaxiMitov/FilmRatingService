using System.Collections.Generic;
// using FilmRatingService.Models; // UserReview and MovieDetails are in the same namespace

namespace FilmRatingService.Models
{
    public class MovieDetailsPageViewModel
    {
        public MovieDetails Movie { get; set; }
        public IEnumerable<UserReview> Reviews { get; set; }

        // <<< ADD PROPERTIES FOR SORTING >>>
        public string CurrentSortOrder { get; set; }
        // For generating sort links in the view
        public string DateSortParam { get; set; }
        public string RatingSortParam { get; set; }


        public MovieDetailsPageViewModel()
        {
            Reviews = new List<UserReview>();
        }
    }
}