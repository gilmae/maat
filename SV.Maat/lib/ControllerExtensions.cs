using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SV.Maat.lib
{
    public static class ControllerExtensions
    {
        public static int? UserId(this Controller controller)
        {
            if (int.TryParse(controller.User.Claims.First(c => c.Type == ClaimTypes.Sid)?.Value, out int userId))
            {
                return userId;
            }
            return null;

        }
        public static int? UserId(this ControllerBase controller)
        {
            if (int.TryParse(controller.User.Claims.First(c => c.Type == ClaimTypes.Sid)?.Value, out int userId))
            {
                return userId;
            }
            return null;

        }
    }
}
