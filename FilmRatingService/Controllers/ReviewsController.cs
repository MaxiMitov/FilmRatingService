using FilmRatingService.Areas.Identity.Data; // For ApplicationUser
using FilmRatingService.Interfaces;         // For IReviewService, IMovieService
using FilmRatingService.Models;             // For AddReviewViewModel, UserReview

using Microsoft.AspNetCore.Authorization;   // For [Authorize]
using Microsoft.AspNetCore.Identity;        // For UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;                               // For DateTime
using System.Security.Claims;               // For User.FindFirstValue (though _userManager.GetUserId is preferred)
using System.Threading.Tasks;

namespace FilmRatingService.Controllers
{
    [Authorize] // Only logged-in users can access review actions
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IMovieService _movieService; // To get movie details for context
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(
            IReviewService reviewService,
            IMovieService movieService,
            UserManager<ApplicationUser> userManager,
            ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _movieService = movieService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Reviews/Create?movieId={movieId}
        [HttpGet]
        public async Task<IActionResult> Create(int movieId)
        {
            if (movieId <= 0)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Home", new { statusCode = 400 });
            }

            var movie = await _movieService.GetMovieDetailsAsync(movieId);
            if (movie == null)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Home", new { statusCode = 404 });
            }

            var viewModel = new AddReviewViewModel
            {
                MovieId = movieId,
                MovieTitle = movie.Title
            };

            return View(viewModel);
        }

        // <<< NEW [HttpPost] Create ACTION METHOD ADDED HERE >>>
        [HttpPost]
        [ValidateAntiForgeryToken] // Important for security to prevent CSRF attacks
        public async Task<IActionResult> Create(AddReviewViewModel viewModel)
        {
            // Re-fetch MovieTitle if ModelState is invalid, as it's not part of the posted data by default
            // unless explicitly included as a hidden field named "MovieTitle" in the form.
            // The Create.cshtml I provided earlier *does* include MovieTitle as a hidden field.
            if (!ModelState.IsValid)
            {
                // If MovieTitle was not posted back and is needed for re-display:
                if (viewModel.MovieId > 0 && string.IsNullOrEmpty(viewModel.MovieTitle))
                {
                    var movieForTitle = await _movieService.GetMovieDetailsAsync(viewModel.MovieId);
                    if (movieForTitle != null) viewModel.MovieTitle = movieForTitle.Title;
                    else viewModel.MovieTitle = "Selected Movie"; // Fallback title
                }
                return View(viewModel); // Return view with validation errors
            }

            var userId = _userManager.GetUserId(User); // Gets the current logged-in user's ID
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found for review submission. User might not be properly authenticated.");
                ModelState.AddModelError(string.Empty, "You must be logged in to submit a review.");
                // Re-populate MovieTitle if needed before returning view
                if (viewModel.MovieId > 0 && string.IsNullOrEmpty(viewModel.MovieTitle))
                {
                    var movieForTitle = await _movieService.GetMovieDetailsAsync(viewModel.MovieId);
                    if (movieForTitle != null) viewModel.MovieTitle = movieForTitle.Title;
                    else viewModel.MovieTitle = "Selected Movie";
                }
                return View(viewModel);
            }

            var userReview = new UserReview
            {
                MovieId = viewModel.MovieId,
                Rating = viewModel.Rating,
                ReviewText = viewModel.ReviewText,
                UserId = userId,
                ReviewDate = DateTime.UtcNow // Set the review date upon creation
            };

            try
            {
                await _reviewService.AddReviewAsync(userReview);
                _logger.LogInformation("Review for MovieId {MovieId} by UserId {UserId} submitted successfully.", viewModel.MovieId, userId);

                TempData["StatusMessage"] = "Your review has been submitted successfully!";

                // Redirect to the movie's details page after successful submission
                return RedirectToAction("Details", "Home", new { id = viewModel.MovieId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving review for MovieId {MovieId} by UserId {UserId}.", viewModel.MovieId, userId);
                ModelState.AddModelError(string.Empty, "An error occurred while submitting your review. Please try again.");
                // Re-populate MovieTitle if returning to the view with an error
                if (viewModel.MovieId > 0 && string.IsNullOrEmpty(viewModel.MovieTitle))
                {
                    var movieForTitle = await _movieService.GetMovieDetailsAsync(viewModel.MovieId);
                    if (movieForTitle != null) viewModel.MovieTitle = movieForTitle.Title;
                    else viewModel.MovieTitle = "Selected Movie";
                }
                return View(viewModel);
            }
        }
    }
}