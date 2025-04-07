using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MultiTaskManager.Model
{

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public int CompanyId { get; set; } // Foreign key to Company

        public Company? Company { get; set; } // Navigation property
        public string RefreshToken { get; set; } // Store refresh token
        public Employee? Employee { get; set; }
        [Required]
        public UserRole Role { get; set; }
        public DateTime RefreshTokenExpiry { get; set; } // Store expiration time for the refresh token
    }
    public enum UserRole
    {
        User,
        ProjectManager,
        Admin,
    }

}
