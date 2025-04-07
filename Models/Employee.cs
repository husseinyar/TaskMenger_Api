using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MultiTaskManager.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-incremented Id
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string JobTitle { get; set; }

        public string? Department { get; set; }

        [Required]
        public int CompanyId { get; set; } // Foreign key to Company

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; } // Navigation property

        public string? UserId { get; set; } // Nullable Foreign Key to ApplicationUser
        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser { get; set; } // Navigation property

        public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>(); // Many-to-Many with Project
    }


}
