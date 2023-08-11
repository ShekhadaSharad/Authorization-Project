namespace DigipTechnologyCP.Class
{
    public class AdminAddUserRole
    {
        private readonly HttpClient _httpClient;

        public AdminAddUserRole(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AdminProvideRoleAsync()
        {
            string apiUrl = "https://localhost:7015/";
            string endpoint = "api/AdminAuthoriz";

            string email = "shekhadasharad24@gmail.com";
            string role = "User";

            try
            {
                var uriBuilder = new UriBuilder(apiUrl + endpoint);
                uriBuilder.Query = $"email={Uri.EscapeDataString(email)}&role={Uri.EscapeDataString(role)}";

                var response = await _httpClient.PostAsync(uriBuilder.Uri, null);

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
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }

}
