using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace SharadDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Hr,Manager")]
    public class RolesController : ControllerBase
    {

        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("Display")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> CreateRole(IdentityRole role)
        {
            if (ModelState.IsValid)
            {

                IdentityRole roles = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = role.Name,
                    ConcurrencyStamp = role.ConcurrencyStamp,
                    NormalizedName = role.NormalizedName,
                };

                var result = await _roleManager.CreateAsync(roles);
                if (result.Succeeded)
                {
                    return CreatedAtAction(nameof(GetRoles), new { id = roles.Id }, roles);
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            return BadRequest(ModelState);
        }
    }
}
