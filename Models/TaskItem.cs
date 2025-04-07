using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MultiTaskManager.Model
{

    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [ForeignKey("Employee")]
        public int? AssignedEmployeeId { get; set; } // Nullable in case a task is not yet assigned
        public Employee AssignedEmployee { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending; // Default status
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed
    }

}

