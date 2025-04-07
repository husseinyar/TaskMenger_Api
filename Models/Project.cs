using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MultiTaskManager.Model
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
