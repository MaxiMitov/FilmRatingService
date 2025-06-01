using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // <<< ADD THIS LINE

namespace FilmRatingService.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // <<< ADD THIS LINE to restrict access to users in the "Admin" role
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}