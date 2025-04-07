using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTaskManager.Data;
using MultiTaskManager.Model;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTaskManager.Controllers
{
    [Route("api/employee-projects")]
    [ApiController]
    [Authorize(Roles = "Admin,UserManager")] // Only Admins & UserManagers can assign projects
    public class EmployeeProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/employee-projects (Get all employee-project assignments)
        [HttpGet]
        public async Task<IActionResult> GetAllAssignments()
        {
            var assignments = await _context.EmployeeProjects
                .Include(ep => ep.Employee)
                .Include(ep => ep.Project)
                .Select(ep => new
                {
                    EmployeeId = ep.Employee.Id,
                    EmployeeName = ep.Employee.FullName,
                    ProjectId = ep.Project.Id,
                    ProjectName = ep.Project.Name
                })
                .ToListAsync();

            return Ok(assignments);
        }

        // ✅ GET: api/employee-projects/{id} (Get assignment by ID)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            var assignment = await _context.EmployeeProjects
                .Include(ep => ep.Employee)
                .Include(ep => ep.Project)
                .Where(ep => ep.EmployeeId == id && ep.ProjectId == id)
                .Select(ep => new
                {
                    EmployeeId = ep.Employee.Id,
                    EmployeeName = ep.Employee.FullName,
                    ProjectId = ep.Project.Id,
                    ProjectName = ep.Project.Name
                })
                .FirstOrDefaultAsync();

            if (assignment == null)
                return NotFound("Assignment not found.");

            return Ok(assignment);
        }

        // ✅ POST: api/employee-projects (Assign an employee to a project)
        [HttpPost]
        public async Task<IActionResult> AssignEmployeeToProject([FromBody] AssignEmployeeProjectModel model)
        {
            var employee = await _context.Employees.FindAsync(model.EmployeeId);
            var project = await _context.Projects.FindAsync(model.ProjectId);

            if (employee == null || project == null)
                return NotFound("Employee or Project not found.");

            // Check if already assigned
            bool exists = await _context.EmployeeProjects
                .AnyAsync(ep => ep.EmployeeId == model.EmployeeId && ep.ProjectId == model.ProjectId);

            if (exists)
                return BadRequest("Employee is already assigned to this project.");

            var assignment = new EmployeeProject
            {
                EmployeeId = model.EmployeeId,
                ProjectId = model.ProjectId
            };

            _context.EmployeeProjects.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee assigned to project successfully." });
        }

        // ✅ DELETE: api/employee-projects/{id} (Remove an employee from a project)
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveEmployeeFromProject(int id)
        {
            var assignment = await _context.EmployeeProjects
                .Where(ep => ep.EmployeeId == id && ep.ProjectId == id)
                .FirstOrDefaultAsync();
            if (assignment == null)
                return NotFound("Assignment not found.");

            _context.EmployeeProjects.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee removed from project successfully." });
        }
    }

    // ✅ Model for Assigning Employees to Projects
    public class AssignEmployeeProjectModel
    {
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
    }
}
