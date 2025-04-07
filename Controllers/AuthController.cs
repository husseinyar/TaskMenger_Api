using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiTaskManager.Data;
using MultiTaskManager.Model;
using MultiTaskManager.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MultiTaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IConfiguration configuration) // Inject IConfiguration to get JWT settings
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _signInManager = signInManager;
            _secret = configuration["Jwt:Secret"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];

        }






        // Get all user data and fatch user role

    
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // Fetch all users
            var users = await _userManager.Users.ToListAsync();

            // Create a list to hold user data with roles
            var usersData = new List<object>();

            // Iterate through each user to get their roles
            foreach (var user in users)
            {
                // Get roles for the current user
                var roles = await _userManager.GetRolesAsync(user);

                // Create a user object with the required properties
                var userData = new
                {
                    user.Id,
                    user.UserName,
                    user.FullName,
                    user.Email,
                    user.CompanyId,
                    Roles = roles // Include roles in the response
                };

                // Add the user data to the list
                usersData.Add(userData);
            }

            // Return the list of users with their roles
            return Ok(usersData);
        }












        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the role exists in the system
            if (!Enum.IsDefined(typeof(UserRole), model.Role))
            {
                return BadRequest("Invalid role selected.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                FullName = model.FullName,
                Email = model.Email,
                CompanyId = model.CompanyId,
                Role = model.Role, // Ensure the role is set in the user object
                RefreshToken = GenerateRefreshToken(), // Initialize RefreshToken
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(30) // Set an initial expiry date
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role.ToString());
                if (!roleResult.Succeeded)
                {
                    // If adding to role fails, delete the created user to maintain consistency
                    await _userManager.DeleteAsync(user);
                    return BadRequest(roleResult.Errors.Select(e => e.Description));
                }
                return Ok(new { user.UserName, user.FullName });
            }

            return BadRequest(result.Errors.Select(e => e.Description));
        }
        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

           
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid login attempt");
            }

            // Generate JWT token
            var token = await GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
            await _userManager.UpdateAsync(user);

            return Ok(new { Token = token, RefreshToken = refreshToken });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logged out successfully" });
        }

        // POST: api/auth/refresh-token
      
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new { Message = "Refresh token is required" });
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return Unauthorized(new { Message = "Invalid or expired refresh token" });
            }

            // Generate new JWT and refresh token
            var newJwtToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Update user with the new refresh token and expiry
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
            await _userManager.UpdateAsync(user);

            return Ok(new { Token = newJwtToken, RefreshToken = newRefreshToken });
        }


        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName),
                new Claim("CompanyId", user.CompanyId.ToString())
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

       [HttpDelete("users/{id}")]
       public async Task<IActionResult> DeleteUser(string id)
{
    // Find the user by ID
    var user = await _userManager.FindByIdAsync(id);
    if (user == null)
    {
        return NotFound("User not found.");
    }

    // Get the roles for the user
    var roles = await _userManager.GetRolesAsync(user);

    // Remove the user from their roles
    if (roles.Any())
    {
        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, roles);
        if (!removeRolesResult.Succeeded)
        {
            return BadRequest("Failed to remove user from roles.");
        }
    }

    // Delete the user
    var deleteResult = await _userManager.DeleteAsync(user);
    if (!deleteResult.Succeeded)
    {
        return BadRequest("Failed to delete user.");
    }

    return Ok("User deleted successfully.");
}

    }


}

/*test User  "fullName": "David",
"username": "David",
  "email": "David@example.com",
  "password": "David12345@",
  "companyId": 1,
  "role": "Admin"*/