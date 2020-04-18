using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.lib;
using SV.Maat.Users.Models;


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

        [HttpGet]
        [Route("{id}")]
        public ActionResult View(long id)
        {
            var user = _userStore.Find(id);
            ViewBag.auth_endpoint = this.Url.ActionLink("AuthenticationRequest", "IndieAuth"); //"https://" + Path.Join(HttpContext.Request.Host.ToString(), "auth");
            ViewBag.token_endpoint = "https://" + Path.Join(HttpContext.Request.Host.ToString(), "auth/token");


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
    }
}
