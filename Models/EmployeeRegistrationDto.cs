using System.ComponentModel.DataAnnotations;

namespace MultiTaskManager.Models
{
    public class EmployeeRegistrationDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string JobTitle { get; set; }

        public string? Department { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class EmployeeUpdateDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string JobTitle { get; set; }

        public string? Department { get; set; }
    }

}
