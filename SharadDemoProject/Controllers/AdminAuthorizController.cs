using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using SharadDemoProject.Controllers.Context;
using SharadDemoProject.Model.Authentication;
using System.Security.Claims;

namespace SharadDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAuthorizController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminAuthorizController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
             IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> AdminProvideRole(string email, string role)
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    Serilog.Log.Error($"User {email} not found.");
                    return StatusCode(StatusCodes.Status404NotFound,
                        new Response { Status = "Error", Message = "User not found." });
                }

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    Serilog.Log.Error($"This Role {role} Does Not Exist. login this user : {userName}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new Response { Status = "Error", Message = "This Role Does Not Exist." });
                }

                // Add the user to the role
                var result = await _userManager.AddToRoleAsync(user, role);

                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new Response { Status = "Success", Message = "User added to the role successfully." });
                }
                else
                {
                    Serilog.Log.Error($"Failed to add user {user} to the role. {role}. login this user : {userName}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new Response { Status = "Error", Message = "Failed to add user to the role." });
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"An error occurred while processing the request. {ex}. login this user : {userName}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "An error occurred while processing the request." });
            }
        }
    }
}
