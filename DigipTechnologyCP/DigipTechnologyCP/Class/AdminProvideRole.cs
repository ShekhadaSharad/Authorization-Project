using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DigipTechnologyCP.Class
{
    public class AdminProvideRole
    {
        public string baseUrl = "https://localhost:7015/";
        private readonly HttpClient _httpClient;


        public AdminProvideRole()
        {
            _httpClient = new HttpClient();
        }

        public async Task AdminGetRole()
        {

            IdentityRole newRole = new IdentityRole
            {
                Name = "Manager",
            };


            string endpoint = "api/Roles/Add";

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonModel = JsonConvert.SerializeObject(newRole);
                var content = new StringContent(jsonModel, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("API request failed. Status code: " + response.StatusCode);
                }
            }
        }
        public async Task RoleDisplay()
        {
            _httpClient.BaseAddress = new Uri(baseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage responseGetRoles = await _httpClient.GetAsync("api/Roles/Display");
            if (responseGetRoles.IsSuccessStatusCode)
            {
                var rolesJson = await responseGetRoles.Content.ReadAsStringAsync();
                Console.WriteLine("Roles: " + rolesJson);
            }
            else
            {
                Console.WriteLine("Error calling GetRoles API: " + responseGetRoles.ReasonPhrase);
            }
        }
    }
}


