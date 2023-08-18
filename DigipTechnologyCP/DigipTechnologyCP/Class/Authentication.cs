using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DigipTechnologyCP.Class
{
    public class Authentication
    {
        public static string? PasswordRegister { get; set; }
        private static readonly string apiUrlWeather = "https://localhost:7015/api/Employee";
        private static readonly string apiUrlLogin = "https://localhost:7015/api/Authentication/login";
        public async Task GetToken()
        {
            var login = new LoginModel();
            Console.Write("Enter username : ");
            login.UserName = "hirenpatel24";
            Console.Write("Enter password : ");
            login.Password = "HiReN@24";

            using (HttpClient client = new HttpClient())
            {
                var loginModel = new { Username = login.UserName, login.Password };
                var response = await client.PostAsJsonAsync(apiUrlLogin, loginModel);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                    login.accessToken = tokenResponse.Token;
                    Console.WriteLine("Access Token Is :{0}\n ", login.accessToken);
                    await Employees(apiUrlWeather, login.accessToken);
                }
                else
                {
                    Console.WriteLine("Login failed with status code: " + response.StatusCode);
                }
            }

        }

        public async Task RegisterUserAsync()
        {
            string apiUrl = "https://localhost:7015/";
            string endpoint = "api/Authentication/RegisterRoleBased";

            var model = new RegisterModel
            {
                Email = "sharad22@gmail.com",
                Username = "itzsharad",
                Password = "HariKrupa@2484"
            };

            string role = "Admin";

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(apiUrl);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonModel = JsonConvert.SerializeObject(model);
                var content = new StringContent(jsonModel, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpoint + $"?role={role}", content);

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
        static async Task Employees(string apiUrlWeather, string accessToken)
        {
            while (true)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    HttpResponseMessage response = await client.GetAsync(apiUrlWeather);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseData + "\n");
                        Console.WriteLine("Access Token Is :{0}\n ", accessToken);
                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + response.StatusCode);
                    }
                }
                Thread.Sleep(100000);
            }
        }
    }
}
