using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ConsumeWebApiAuth.Controllers
{
    public class RolesCunsumerController : Controller
    {
        public async Task<IActionResult> Index()
        {
            List<IdentityRole> roles = await GetRolesFromAPI();
            return View(roles);
        }

        private async Task<List<IdentityRole>> GetRolesFromAPI()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://localhost:7015/"); 
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await httpClient.GetAsync("api/Roles");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var roles = JsonConvert.DeserializeObject<List<IdentityRole>>(content);
                    return roles;
                }
                else
                {
                    return new List<IdentityRole>();
                }
            }
        }

        public IActionResult Create()
        {
            return View(new IdentityRole());
        }

        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://localhost:7015/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    if (ModelState.IsValid)
                    {
                        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/Roles", role);
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            var createdRole = JsonConvert.DeserializeObject<IdentityRole>(jsonResponse);

                            return CreatedAtAction("GetRoles", new { id = createdRole.Id }, createdRole);
                        }
                        else
                        {
                            return StatusCode((int)response.StatusCode, "Failed to create role. Please try again later.");
                        }
                    }
                    else
                    {
                        return View(role);
                    }
                }
                catch (HttpRequestException ex)
                {
                    return StatusCode(500, "Failed to create role. Please try again later.");
                }
            }
        }
    }
}
