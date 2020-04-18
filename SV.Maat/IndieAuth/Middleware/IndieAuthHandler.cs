using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SV.Maat.IndieAuth.Middleware
{
    public class IndieAuthHandler
    {
        private readonly RequestDelegate _next;
        private readonly IAuthenticationRequestStore _authenticationRequestStore;

        public IndieAuthHandler(RequestDelegate next, IAuthenticationRequestStore authenticationRequestStore)
        {
            _next = next;
            _authenticationRequestStore = authenticationRequestStore;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string accessToken = string.Empty;
            if (context.Request.ContentType == "application/x-www-form-urlencoded" || context.Request.ContentType == "multipart/form-data")
            {
                accessToken = context.Request.Form["access-token"];
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = context.Request.Headers["Authorization"];
                accessToken = accessToken.Substring("Bearer ".Length); // Slice off the 'Bearer ' prefix
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }

            string hashedToken = BCrypt.Net.BCrypt.HashPassword(accessToken);
            var authRequest =_authenticationRequestStore.FindByAccessToken(hashedToken);

            if (authRequest == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }

                // Call the next delegate/middleware in the pipeline
                await _next(context);
        }
    }

    //public static class IndieAuthHandlerExtensions
    //{
    //    public static IApplicationBuilder UseIndieAuth(this IApplicationBuilder builder)
    //    {
    //        return builder.UseMiddleware<IndieAuthHandler>();
    //    } 
    //}
}
