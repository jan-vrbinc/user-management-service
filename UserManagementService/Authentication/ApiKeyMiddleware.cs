using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using UserManagementService.Data;

namespace UserManagementService.Authentication
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, IHostEnvironment env)
        {
            // Bypass authentication for Swagger endpoints and requests originating from Swagger in Development
            if (env.IsDevelopment())
            {
                if (context.Request.Path.StartsWithSegments("/swagger") ||
                    (context.Request.Headers.TryGetValue("Referer", out var referer) && 
                     referer.ToString().Contains("/swagger", StringComparison.OrdinalIgnoreCase)))
                {
                    await _next(context);
                    return;
                }
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            var apiKey = extractedApiKey.ToString();
            
            // Check if the API key exists in the database
            var isValid = await dbContext.ApiClients.AnyAsync(c => c.ApiKey == apiKey);

            if (!isValid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}

