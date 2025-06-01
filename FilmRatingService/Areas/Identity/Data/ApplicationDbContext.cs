using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FilmRatingService.Areas.Identity.Data; // Include your namespace for ApplicationUser

namespace FilmRatingService.Areas.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // this must match your user class
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add this if you're doing anything with users directly (optional but common)
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }
}
