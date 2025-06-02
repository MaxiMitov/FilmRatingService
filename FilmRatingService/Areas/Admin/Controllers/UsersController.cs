using FilmRatingService.Areas.Identity.Data; // For ApplicationUser

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Required for ToListAsync() on IQueryable users
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmRatingService.Areas.Admin.Controllers
{
    [Area("Admin")] // Specify the area
    [Authorize(Roles = "Admin")] // Restrict access to users in the "Admin" role
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
            // Retrieve all users. UserManager.Users returns an IQueryable<ApplicationUser>.
            // We convert it to a list to pass to the view.
            var users = await _userManager.Users.ToListAsync();

            // For a more detailed view, you might map these to a ViewModel
            // For now, we'll pass the ApplicationUser objects directly.
            // Consider what information you want to display: Id, UserName, Email, Name (custom property), roles?

            // Example of creating a simple ViewModel list if needed (optional for now):
            // var userViewModels = new List<UserViewModel>();
            // foreach (var user in users)
            // {
            //     userViewModels.Add(new UserViewModel
            //     {
            //         Id = user.Id,
            //         UserName = user.UserName,
            //         Email = user.Email,
            //         FullName = user.Name // Your custom Name property
            //         // Roles = await _userManager.GetRolesAsync(user) // Getting roles can be an N+1 query, handle with care
            //     });
            // }
            // return View(userViewModels);

            _logger.LogInformation("Admin Users/Index page accessed. Displaying {UserCount} users.", users.Count);
            return View(users); // Pass the list of ApplicationUser to the view
        }

        // Placeholder for a UserViewModel if you decide to use one
        // public class UserViewModel
        // {
        //     public string Id { get; set; }
        //     public string UserName { get; set; }
        //     public string Email { get; set; }
        //     public string FullName { get; set; } // From your ApplicationUser.Name
        //     // public IList<string> Roles { get; set; }
        // }
    }
}