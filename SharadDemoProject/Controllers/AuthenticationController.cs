using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SharadDemoProject.Controllers.Context;
using SharadDemoProject.Model.Authentication;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User.Management.Service.Services;

namespace SharadDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly EmailServices _emailService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            EmailServices emailService,
            ILogger<AuthenticationController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("RegisterRoleBased")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model, string role)
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

            var userExist = await _userManager.FindByEmailAsync(model.Email);
            if (userExist != null)
            {
                Serilog.Log.Warning($"User with email {model.Email} already exists. login this user : {userName}");
                return StatusCode(StatusCodes.Status403Forbidden,
                    new Response { Status = "Error", Message = "User already Exists!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            if (await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    Serilog.Log.Error($"HTTP : {Request.Method} : {Request.Path} responded : {Response.StatusCode}.User creation failed! {model.Email} Please check user details and try again. login this user : {userName}");
                    return BadRequest(new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
                }
                await _userManager.AddToRoleAsync(user, role);
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "SuccessFully", Message = "User Created SuccessFully" });

            }
            else
            {
                Serilog.Log.Error($"HTTP : {Request.Method} : {Request.Path} responded : {Response.StatusCode}. This Role Doesnot Exsit{role}. login this user : {userName}");
                return BadRequest(new Response { Status = "Error", Message = "This Role Doesnot Exsit" });
            }
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            try
            {
                var userExists = await _userManager.FindByNameAsync(model.Username);
                if (userExists != null)
                {
                    Serilog.Log.Error($"HTTP : {Request.Method} : {Request.Path} responded : {Response.StatusCode}. User {model.Username} already exists!");
                    return BadRequest(new Response {  Status = "Error", Message = "User already exists!" });
                }

                ApplicationUser user = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    Serilog.Log.Error($"HTTP : {Request.Method} : {Request.Path} responded : {Response.StatusCode}. User creation failed for {model.Username},{model.Email},{model.Password}. login this user : {userName}");
                    return BadRequest(new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
                }

                return Ok(new Response { Status = "Success", Message = "User created successfully!" });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"HTTP : {Request.Method} : {Request.Path} responded : {Response.StatusCode}. An error occurred during user registration{ex}. login this user : {userName}");
                return BadRequest(new Response { Status = "Error", Message = "An error occurred during user registration." });
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                    {
                         new Claim(ClaimTypes.Name, user.UserName),
                         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var token = CreateToken(authClaims);
                    var refreshToken = GenerateRefreshToken();


                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(5);

                    await _userManager.UpdateAsync(user);

                    return Ok(new
                    {
                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                        RefreshToken = refreshToken,
                        Expiration = token.ValidTo
                    });
                }
                Serilog.Log.Warning("User Unauthorized!");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"HTTP : {Request.Method} : {Request.Path} responded : {Response.StatusCode} An error occurred while processing the request.{ex}.  login this user : {userName} ");
                return BadRequest(new Response { Status = "Error", Message = "An error occurred during user registration." });
            }
        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(50),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
              

            return principal;
        }
    }
}
