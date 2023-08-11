using ConsumeWebApiAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharadDemoProject.Model.Employees;
using System.Diagnostics;
using System.Net;

namespace ConsumeWebApiAuth.Controllers
{
    public class EmployeeCunsume : Controller
    {
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            List<EmployeeModel> employees = new List<EmployeeModel>();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:7015/");

                    HttpResponseMessage response = await client.GetAsync("api/Employee");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        employees = JsonConvert.DeserializeObject<List<EmployeeModel>>(result)!;
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        ViewBag.Message = "No employees found.";
                    }
                    else
                    {
                        ViewBag.Message = "An error occurred while retrieving employees.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
            }

            return View(employees);
        }

        public async Task<IActionResult> Details(int id)
        {
            EmployeeModel employees = new EmployeeModel();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:7015/");
                HttpResponseMessage response = await client.GetAsync($"api/Employee/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    employees = JsonConvert.DeserializeObject<EmployeeModel>(result)!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return View(employees);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:7015/");
                HttpResponseMessage response = await client.DeleteAsync($"api/Employee/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return View(new EmployeeModel());
        }

        private static async Task<EmployeeModel> GetEmployee(int id)
        {
            EmployeeModel employees = new EmployeeModel();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:7015/");
                HttpResponseMessage response = await client.GetAsync($"api/Employee/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    employees = JsonConvert.DeserializeObject<EmployeeModel>(result)!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return employees;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeModel employee)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:7015/");

                var response = await client.PostAsJsonAsync("api/Employee/Create", employee);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return View("Error");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            EmployeeModel employees = await GetEmployee(id);
            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeModel employee)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:7015/");
                var response = await client.PutAsJsonAsync($"api/Employee/{employee.EmpId}", employee);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //public IActionResult GoogleLogin()
        //{
        //    string redirectUrl = Url.Action("GoogleResponse", "Home");
        //    var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        //    return new ChallengeResult("Google", properties);
        //}

        //public async Task<IActionResult> GoogleResponse()
        //{
        //    ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
        //    if (info == null)
        //        return RedirectToAction(nameof(Login));

        //    var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        //    string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };
        //    if (result.Succeeded)
        //        return View(userInfo);
        //    else
        //    {
        //        AppUser user = new AppUser
        //        {
        //            Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
        //            UserName = info.Principal.FindFirst(ClaimTypes.Email).Value
        //        };

        //        IdentityResult identResult = await userManager.CreateAsync(user);
        //        if (identResult.Succeeded)
        //        {
        //            identResult = await userManager.AddLoginAsync(user, info);
        //            if (identResult.Succeeded)
        //            {
        //                await signInManager.SignInAsync(user, false);
        //                return View(userInfo);
        //            }
        //        }
        //        return AccessDenied();
        //    }
        //}
        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}
    }
    //[HttpGet]
    //[AllowAnonymous]
    //public async Task<IActionResult> Login(string returnUrl)
    //{

    //    LoginModel login = new LoginModel
    //    {
    //        ReturnUrl = returnUrl,
    //        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemeAsync()).,
    //    };
    //    return View();
    //}
}