using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.lib;
using SV.Maat.Users.Models;


namespace SV.Maat.Users
{
    [Route("user")]
    public class UserController : ControllerBase
    {
        IUserStore _userStore;

        public UserController(IUserStore userStore)
        {
            _userStore = userStore;
        }

        //[HttpPost]
        //[Route("register")]
        //public IActionResult Register([FromBody] Credentials model)
        //{

        //    User user = new User { Username = model.Username };
        //    user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password, BCrypt.Net.BCrypt.GenerateSalt());

        //    _userStore.Insert(user);

        //    return Ok(user);
        //}

        //[HttpPost]
        //[Route("login")]
        //public IActionResult Login([FromBody] Credentials model)
        //{
        //    User user = _userStore.FindByUsername(model.Username).FirstOrDefault(u=> BCrypt.Net.BCrypt.Verify(model.Password, u.HashedPassword));

        //    if (user != null)
        //    {
        //        return Ok();
        //    }
        //    return Unauthorized();
        //}

        [HttpGet]
        [Route("")]
        public IActionResult GetUserPage()
        {
            return Ok(new User() { Name = "gilmae" });

        }


    }
}
