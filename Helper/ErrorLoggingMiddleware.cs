using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DominikStiller.VertretungsplanServer.Helper
{
    // https://blog.elmah.io/error-logging-middleware-in-aspnetcore/
    public class ErrorLoggingMiddleware
    {
        readonly RequestDelegate next;
        readonly ILogger logger;

        public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                logger.LogError($"ERROR while handling request\n{e}");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }

    public static class ErrorLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorLoggingMiddleware>();
        }
    }
}
