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

        // <<< NEW [HttpPost] Delete ACTION METHOD ADDED HERE >>>
        [HttpPost]
        [ValidateAntiForgeryToken] // Good practice for actions that modify data
        public async Task<IActionResult> Delete(int id) // 'id' will be the reviewId
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid review ID provided for deletion.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Admin attempting to delete review with Id: {ReviewId}", id);
            var success = await _reviewService.DeleteReviewAsync(id);

            if (success)
            {
                TempData["StatusMessage"] = $"Review with ID {id} has been successfully deleted.";
                _logger.LogInformation("Admin successfully deleted review with Id: {ReviewId}", id);
            }
            else
            {
                // The service might return false if the review wasn't found or if another error occurred during deletion.
                // The ReviewService itself logs specific errors related to the deletion attempt.
                TempData["ErrorMessage"] = $"Could not delete review with ID {id}. It might have already been deleted, or an error occurred. Check logs for details.";
                _logger.LogWarning("Admin failed to delete review with Id: {ReviewId}. Review might not exist or service returned false.", id);
            }

            return RedirectToAction(nameof(Index)); // Redirect back to the list of all reviews
        }

        // Optional: For a "soft delete" or a more user-friendly delete with a confirmation page,
        // you would typically add an HttpGet Delete action as well:
        //
        // [HttpGet]
        // public async Task<IActionResult> Delete(int id)
        // {
        //     if (id <= 0)
        //     {
        //         return NotFound();
        //     }
        //     var review = await _reviewService.GetReviewByIdAsync(id); // Using the method from IReviewService
        //     if (review == null)
        //     {
        //         TempData["ErrorMessage"] = $"Review with ID {id} not found.";
        //         return RedirectToAction(nameof(Index));
        //     }
        //     // You would create a new View: Areas/Admin/Views/ReviewsAdmin/Delete.cshtml
        //     // This view would display review details and ask "Are you sure you want to delete this?"
        //     // It would have a form that POSTs to the HttpPost Delete action (which might be renamed DeleteConfirmed).
        //     return View(review); 
        // }
        //
        // If you add the HttpGet Delete action, you might rename the HttpPost action to DeleteConfirmed(int id)
        // and add [ActionName("Delete")] to the HttpPost attribute:
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(int id) { /* ... actual delete logic ... */ }
    }
}