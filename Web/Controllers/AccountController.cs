using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

using DominikStiller.VertretungsplanServer.Web.Helper;

namespace DominikStiller.VertretungsplanServer.Web.Controllers
{
    public class AccountController : Controller
    {
        readonly UserRepository users;

        public AccountController(UserRepository users)
        {
            this.users = users;
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                if (HttpContext.User.IsInRole("Student"))
                    return Redirect("/students");
                else
                    return Redirect("/teachers");
            }

            return View();
        }

        [HttpPost]
        [Route("/")]
        public async Task<IActionResult> Login(string username, string password, bool rememberme)
        {
            var user = users.Authenticate(username, password);

            if (user != null)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Type.ToString())
                };

                var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Password"));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, new AuthenticationProperties()
                {
                    IsPersistent = rememberme
                });

                // Redirects to Vertretungsplan page
                return Redirect("/");
            }
            else
            {
                ViewData["Error"] = true;
                return View();
            }
        }

        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("/");
        }
    }
}
