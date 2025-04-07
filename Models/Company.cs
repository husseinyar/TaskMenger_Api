using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MultiTaskManager.Model
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(150)] // Limit company name length
        public string Name { get; set; }

        [Required]
        [StringLength(250)]
        public string Address { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone] // Ensures a valid phone number format
        public string PhoneNumber { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
