using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;

namespace FilmRatingService.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
