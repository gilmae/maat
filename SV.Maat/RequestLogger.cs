using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace SV.Maat
{
    public class RequestLogger
    {
        private readonly RequestDelegate _next;
        ILogger<RequestLogger> _logger;

        public RequestLogger(RequestDelegate next, ILogger<RequestLogger> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint() as RouteEndpoint;
            var routeTemplate = string.Empty;
            if (endpoint != null)
            {
                routeTemplate = endpoint.RoutePattern.RawText;

                _logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(new
                {
                    EndPoint = $"{context.Request.Method.ToUpper()} {routeTemplate}",
                    DateRequested = DateTime.UtcNow
                }));
            }
            await _next(context);
        }
    }

    public static class RequestLoggerUtils
    {
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLogger>();
        }
    }
}
