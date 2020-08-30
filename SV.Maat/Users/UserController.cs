using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.Users.Models;
using Users;


namespace SV.Maat.Users
{
    [Route("user")]
    public class UsersController : Controller
    {
        IUserStore _userStore;

        public UsersController(IUserStore userStore)
        {
            _userStore = userStore;
        }


        [HttpHead]
        [Route("{username}")]
        public ActionResult Head(string username)
        {
            var user = _userStore.FindByUsername(username).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpGet]
        [Route("{username}")]
        public new ActionResult View(string username)
        {
            var user = _userStore.FindByUsername(username).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }
            ViewBag.auth_endpoint = this.Url.ActionLink("AuthenticationRequest", "IndieAuth"); //"https://" + Path.Join(HttpContext.Request.Host.ToString(), "auth");
            ViewBag.token_endpoint = this.Url.ActionLink("TokenRequest", "IndieAuth");


            return View(user);
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create([FromBody]Credentials model)
        {
            var user = new User { Username = model.Username, Name = model.Username, HashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password) };
            _userStore.Insert(user);
            return Ok(user);
        }

        [HttpGet]
        [Route("signin")]
        public IActionResult SigninForm([FromQuery] string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> SigninAsync([FromForm]string username, [FromForm] string password, [FromForm] string returnUrl = "")
        {
            var users = _userStore.FindByUsername(username);
            var user = users.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(password, u.HashedPassword));

            var claims = new[]
            {
                new Claim(ClaimTypes.Sid, user.id.ToString()),
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("View", "Users", new { user.id });
        }

        [HttpPost]
        [Route("signout")]
        public async Task<IActionResult> Signout()
        {
            await this.HttpContext.SignOutAsync();
            return RedirectToAction("SigninForm", "User", null);
        }
    }
}