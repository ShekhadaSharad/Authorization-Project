using DigipTechnologyCP.Class;
using System.Net.Http;

namespace DigipTechnologyCP
{
    public class TokenResponse
    {
        public string Token { get; set; }
    }
    public class LoginModel
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? accessToken { get; set; }
    }
    public class RegisterModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class Response
    {
        public string Message { get; set; }
    }

    public class Program
    {

        public static async Task Main(string[] args)
        {

            Console.WriteLine("Register 1 :\n" +
                "Login 2 : \n" +
                "Create Role 3 : \n" +
                "Display Role 4 : \n" +
                "Admin Provider User to Role 5 : \n" +
                " ");
            int no = Convert.ToInt32(Console.ReadLine());
            Authentication program = new Authentication();
            switch (no)
            {
                case 1:
                    await program.RegisterUserAsync();
                    break;
                case 2:

                    await program.GetToken();
                    break;
                case 3:
                    AdminProvideRole adminProvideRole = new AdminProvideRole();
                    await adminProvideRole.AdminGetRole();
                    break;
                case 4:
                    AdminProvideRole DisplayRole = new AdminProvideRole();
                    await DisplayRole.RoleDisplay();
                    break;
                case 5:
                    using (var httpClient = new HttpClient())
                    {
                        AdminAddUserRole adminUserRole = new AdminAddUserRole(httpClient);
                        await adminUserRole.AdminProvideRoleAsync();
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }
}