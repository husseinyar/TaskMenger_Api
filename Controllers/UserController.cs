
    using MultiTaskManager.Data;
    using MultiTaskManager.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
   

    namespace MultiTaskManager.Controllers
    {
        [Route("api/users")]
        [ApiController]
        [Authorize(Roles = "Admin,UserManager")] // Only Admins & UserManagers can manage users
        public class UserController : ControllerBase
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ApplicationDbContext _context;

            public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _context = context;
            }

            // ✅ GET: api/users (Get All Users)
            [HttpGet]
            public async Task<IActionResult> GetAllUsers()
            {
                var users = await _userManager.Users.Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FullName,
                    u.Email,
                    u.CompanyId,
                    Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault()
                }).ToListAsync();

                return Ok(users);
            }

            // ✅ GET: api/users/{id} (Get User by ID)
            [HttpGet("{id}")]
            public async Task<IActionResult> GetUserById(string id)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound("User not found.");

                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.FullName,
                    user.Email,
                    user.CompanyId,
                    Role = role
                });
            }

            // ✅ PUT: api/users/{id} (Update User Details)
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel model)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound("User not found.");

                user.FullName = model.FullName ?? user.FullName;
                user.Email = model.Email ?? user.Email;
                user.CompanyId = model.CompanyId ?? user.CompanyId;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) return BadRequest(result.Errors);

                return Ok(new { message = "User updated successfully." });
            }

            // ✅ DELETE: api/users/{id} (Delete User)
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteUser(string id)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound("User not found.");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded) return BadRequest(result.Errors);

                return Ok(new { message = "User deleted successfully." });
            }

            // ✅ POST: api/users/assign-role (Assign Role to User)
            [HttpPost("assign-role")]
            public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null) return NotFound("User not found.");

                if (!await _roleManager.RoleExistsAsync(model.Role))
                    return BadRequest("Invalid role.");

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                var result = await _userManager.AddToRoleAsync(user, model.Role);

                if (!result.Succeeded) return BadRequest(result.Errors);

                return Ok(new { message = $"User assigned to role {model.Role} successfully." });
            }
        }

        // ✅ Models for Request Body
        public class UpdateUserModel
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public int? CompanyId { get; set; }
        }

        public class AssignRoleModel
        {
            public string UserId { get; set; }
            public string Role { get; set; }
        }
    }


