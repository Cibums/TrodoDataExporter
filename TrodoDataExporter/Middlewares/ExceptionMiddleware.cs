using System;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TrodoDataExporter.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e, "An AmazonS3Exception occurred.");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"AmazonS3Exception: {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred.");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"An error occurred: {e.Message}");
            }
        }
    }
}
