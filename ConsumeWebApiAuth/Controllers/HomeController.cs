using ConsumeWebApiAuth.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text;
using SharadDemoProject.Model.Authentication;

namespace ConsumeWebApiAuth.Controllers
{
    public class HomeController : Controller
    {

        private readonly HttpClient _httpClient;

        public class LoginInput
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }
        public HomeController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7015/api/");
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CookiesView()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(LoginInput input)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Append("Username", input.Username, cookieOptions);
            ViewBag.Save = "Cookie Saved";


            return View();
        }
        public IActionResult ReadCookie()
        {
            ViewBag.Email = Request.Cookies["Email"].ToString();
            return View("Create");
        }

        [HttpPost]
        public async Task<ActionResult> CookiesView(LoginInput loginInput)
        {
            try
            {
                var loginModel = new LoginModel
                {
                    Username = loginInput.Username,
                    Password = loginInput.Password
                };

                var json = JsonConvert.SerializeObject(loginModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Authentication/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginInput.Username),
                        // Add more claims as needed
                    };

                    var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(claimsIdentity);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,

                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    };

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties).Wait();

                    return View("Success");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return View("Unauthorized");
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
    }
}