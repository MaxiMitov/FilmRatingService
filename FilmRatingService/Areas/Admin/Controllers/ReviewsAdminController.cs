using FilmRatingService.Interfaces; // For IReviewService

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace FilmRatingService.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Ensure only Admins can access
    public class ReviewsAdminController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsAdminController> _logger;

        public ReviewsAdminController(IReviewService reviewService, ILogger<ReviewsAdminController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // GET: /Admin/ReviewsAdmin or /Admin/ReviewsAdmin/Index
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin ReviewsAdmin/Index page accessed.");
            var allReviews = await _reviewService.GetAllReviewsAsync();
            // We will pass List<UserReview> to the view for now.
            // Later, you might map this to a specific AdminReviewViewModel.
            return View(allReviews); // This will look for Areas/Admin/Views/ReviewsAdmin/Index.cshtml
        }

        // Future actions for managing reviews (e.g., Delete, EditDetails) can go here
    }
}