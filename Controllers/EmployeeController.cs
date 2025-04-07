using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTaskManager.Data;
using MultiTaskManager.Model;
using MultiTaskManager.Models;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,UserManager")] // Only Admins and UserManagers can manage employees
public class EmployeeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ✅ Get all employees
    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _context.Employees
            .Include(e => e.ApplicationUser)
            .Include(e => e.Company)
            .ToListAsync();
        return Ok(employees);
    }

    // ✅ Get employee by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.ApplicationUser)
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        return Ok(employee);
    }

    // ✅ Admin registers a new employee and assigns a user account
    [HttpPost("register")]
    public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeRegistrationDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Email is already in use" });

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            CompanyId = model.CompanyId,
           
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var employee = new Employee
        {
            FullName = model.FullName,
            JobTitle = model.JobTitle,
            Department = model.Department,
            CompanyId = model.CompanyId,
            UserId = user.Id
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee registered successfully" });
    }

    // ✅ Update an employee (Admin or UserManager only)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto model)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        employee.FullName = model.FullName;
        employee.JobTitle = model.JobTitle;
        employee.Department = model.Department;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee updated successfully" });
    }

    // ✅ Delete an employee (Only if necessary)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee deleted successfully" });
    }
}
