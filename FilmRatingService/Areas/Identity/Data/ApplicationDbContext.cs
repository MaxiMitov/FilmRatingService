using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FilmRatingService.Areas.Identity.Data; // Your namespace for ApplicationUser
using FilmRatingService.Models; // <<< ADD THIS for UserReview

namespace FilmRatingService.Areas.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add this DbSet for your new UserReview entity
        public DbSet<UserReview> UserReviews { get; set; } // <<< ADD THIS LINE

        // Your existing DbSet for ApplicationUsers (optional but common)
        // public DbSet<ApplicationUser> ApplicationUsers { get; set; } // This was in your original file, ensure it's still intended as IdentityDbContext<ApplicationUser> already provides access to users via this.Users

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // You can configure entity relationships here if needed,
            // but the [ForeignKey] attribute in UserReview often suffices for simple cases.
            // Example for UserReview and ApplicationUser relationship (often automatically handled by conventions):
            // builder.Entity<UserReview>()
            //    .HasOne(ur => ur.User)
            //    .WithMany() // If ApplicationUser doesn't have a collection of UserReviews
            //    .HasForeignKey(ur => ur.UserId)
            //    .IsRequired();

            // Ensure MovieId is not treated as a foreign key to a non-existent Movie entity in this DbContext
            // unless you plan to also store Movie entities locally.
            // For now, MovieId is just an integer.
        }
    }
}