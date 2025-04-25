using FilmRatingService.Models;

using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

namespace FilmRatingService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var featured = new FeaturedMovieViewModel
            {
                Title = "Diego Luna on Cassian's Growth in \"Andor\" Season 2",
                Description = "Watch Our Star Wars Celebration Interview",
                CoverImageUrl = "/images/andor-promo.png", 
                VideoUrl = "#",
                Rating = 9.2,
                Likes = 21,
                Hearts = 14
            };

            return View(featured);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
