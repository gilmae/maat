using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Honeycomb.AspNetCore;
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
        IHoneycombEventManager _eventManager;

        public RequestLogger(RequestDelegate next, ILogger<RequestLogger> logger, IHoneycombEventManager eventManager)
        {
            _next = next;
            _logger = logger;
            _eventManager = eventManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var endpoint = context.GetEndpoint() as RouteEndpoint;

            Dictionary<string, object> data = new Dictionary<string, object>();

            data.TryAdd("request.path", context.Request.Path.Value);
            data.TryAdd("request.method", context.Request.Method);
            data.TryAdd("request.http_version", context.Request.Protocol);
            data.TryAdd("request.content_length", context.Request.ContentLength);
            data.TryAdd("request.header.x_forwarded_proto", context.Request.Scheme);
            data.TryAdd("meta.local_hostname", Environment.MachineName);

            try
            {
                await _next(context);
                data.TryAdd("request.endpoint", $"{context.Request.Method.ToUpper()} {endpoint?.RoutePattern.RawText}");

                data.TryAdd("name", $"{context.GetRouteValue("controller")}#{context.GetRouteValue("action")}");
                data.TryAdd("action", context.GetRouteValue("action"));
                data.TryAdd("controller", context.GetRouteValue("controller"));
                data.TryAdd("response.content_length", context.Response.ContentLength);
                data.TryAdd("response.status_code", context.Response.StatusCode);
                data.TryAdd("duration_ms", stopwatch.ElapsedMilliseconds);

                _eventManager.AddData("api_response_ms", stopwatch.ElapsedMilliseconds);
            }
            catch(Exception ex)
            {
                data.TryAdd("request.error", ex.Source);
                data.TryAdd("request.error_detail", ex.Message);

                _eventManager.AddData("request.error", ex.Source);
                _eventManager.AddData("request.error_detail", ex.Message);

                throw;
            }
            finally
            {
                _logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(data));
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
