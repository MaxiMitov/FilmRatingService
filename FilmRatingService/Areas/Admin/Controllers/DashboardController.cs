using Microsoft.AspNetCore.Mvc;

namespace FilmRatingService.Areas.Admin.Controllers
{
    [Area("Admin")] // Crucial: Specifies this controller belongs to the "Admin" area
    // Later, we'll add [Authorize(Roles = "Admin")] here
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}