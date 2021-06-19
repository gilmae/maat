using System;
using System.Diagnostics;
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

            Activity.Current.AddTag("request.http_version", context.Request.Protocol);
            Activity.Current.AddTag("request.content_length", context.Request.ContentLength);
            Activity.Current.AddTag("request.header.x_forwarded_proto", context.Request.Scheme);
            Activity.Current.AddTag("meta.local_hostname", Environment.MachineName);

            try
            {
                await _next(context);
                Activity.Current.AddTag("request.endpoint", $"{context.Request.Method.ToUpper()} {endpoint?.RoutePattern.RawText}");

                Activity.Current.AddTag("name", $"{context.GetRouteValue("controller")}#{context.GetRouteValue("action")}");
                Activity.Current.AddTag("action", context.GetRouteValue("action"));
                Activity.Current.AddTag("controller", context.GetRouteValue("controller"));
                Activity.Current.AddTag("response.content_length", context.Response.ContentLength);
                Activity.Current.AddTag("response.status_code", context.Response.StatusCode);
            }
            catch(Exception ex)
            {
                Activity.Current.AddTag("request.error", ex.Source);
                Activity.Current.AddTag("request.error_detail", ex.Message);

                throw;
            }
            finally
            {
                //_logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(data));
                
            }
           
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
