using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Mvc.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]

    public class CookiesController : Controller
    {
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction ("CookiesView", "Home");
        }
    }
}
