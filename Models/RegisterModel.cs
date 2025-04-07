using MultiTaskManager.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MultiTaskManager.Models
{
    public class RegisterModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public int CompanyId { get; set; } // Foreign key to Company

       [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; } // Role: Admin, Employee, UserManager

    }

   
}
