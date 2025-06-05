using FilmRatingService.Areas.Identity.Data; // For ApplicationUser [cite: maximitov/filmratingservice/FilmRatingService-fbc3c10abc252b3854411cc726b0216eb4002661/FilmRatingService/Areas/Identity/Data/ApplicationUser.cs]
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Required for UserManager, IdentityResult
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq; // Required for .Any()
using System.Threading.Tasks;
using System; // Required for StringSplitOptions

namespace FilmRatingService.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<ApplicationUser> userManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Admin/Users or /Admin/Users/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            _logger.LogInformation("Admin Users/Index page accessed. Displaying {UserCount} users.", users.Count);
            return View(users);
        }

        // <<< NEW [HttpPost] DeleteUser ACTION METHOD ADDED HERE >>>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id) // User ID is a string for Identity
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID was not provided for deletion.";
                return RedirectToAction(nameof(Index));
            }

            var userToDelete = await _userManager.FindByIdAsync(id);

            if (userToDelete == null)
            {
                TempData["ErrorMessage"] = $"User with ID {id} not found.";
                _logger.LogWarning("Admin attempt to delete non-existent user with ID: {UserId}", id);
                return RedirectToAction(nameof(Index));
            }

            // CRITICAL: Prevent admin from deleting their own account
            var currentUserId = _userManager.GetUserId(User);
            if (userToDelete.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own administrator account.";
                _logger.LogWarning("Admin (UserId: {AdminUserId}) attempted to delete their own account (TargetUserId: {TargetUserId}). Action prevented.", currentUserId, userToDelete.Id);
                return RedirectToAction(nameof(Index));
            }

            // Optional: Prevent deletion if user is the *only* admin left (more complex logic)
            // var isAdmin = await _userManager.IsInRoleAsync(userToDelete, "Admin");
            // if (isAdmin)
            // {
            //     var admins = await _userManager.GetUsersInRoleAsync("Admin");
            //     if (admins.Count <= 1)
            //     {
            //         TempData["ErrorMessage"] = "Cannot delete the last administrator account.";
            //         return RedirectToAction(nameof(Index));
            //     }
            // }

            _logger.LogInformation("Admin (UserId: {AdminUserId}) attempting to delete user with ID: {TargetUserId}, Username: {TargetUsername}", currentUserId, userToDelete.Id, userToDelete.UserName);
            IdentityResult result = await _userManager.DeleteAsync(userToDelete);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = $"User {userToDelete.UserName} (ID: {id}) has been successfully deleted.";
                _logger.LogInformation("Admin (UserId: {AdminUserId}) successfully deleted user with ID: {TargetUserId}, Username: {TargetUsername}", currentUserId, userToDelete.Id, userToDelete.UserName);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"Could not delete user {userToDelete.UserName} (ID: {id}). Errors: {errors}";
                _logger.LogWarning("Admin (UserId: {AdminUserId}) failed to delete user with ID: {TargetUserId}, Username: {TargetUsername}. Errors: {Errors}", currentUserId, userToDelete.Id, userToDelete.UserName, errors);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}