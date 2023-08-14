using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharadDemoProject.Controllers.Context;
using SharadDemoProject.Model.Authentication;
using System.Security.Claims;

namespace SharadDemoProject.Controllers
{

    public class CustomAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomAuthorizeFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            try
            {
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    Serilog.Log.Error($"You are not authorized to access this resource : {context}. login this user : {userName}");
                    context.Result = new UnauthorizedObjectResult(
                        new { Status = "Error", Message = "You are not authorized to access this resource." });
                }
                else if (!context.HttpContext.User.IsInRole("Admin"))
                {
                    context.Result = new ObjectResult(
                    new { Status = "Unauthorized", Message = "Only Access Admin " })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
            catch (Exception ex)
            {
                 Serilog.Log.Error($"An error occurred while processing the request{ex} . login this user : {userName}");
                context.Result = new ObjectResult(
                    new { Status = "Error", Message = "An error occurred while processing the request." })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(CustomAuthorizeFilter))]
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
