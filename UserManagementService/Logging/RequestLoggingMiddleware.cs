using System.Diagnostics;
using System.Net;
using System.Text;
using Serilog;

namespace UserManagementService.Logging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestTime = DateTime.UtcNow;

            // Capture request body if needed (careful with large bodies)
            string requestBody = string.Empty;
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                 context.Request.EnableBuffering();
                 using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                 {
                     requestBody = await reader.ReadToEndAsync();
                     context.Request.Body.Position = 0;
                 }
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception has occurred while executing the request.");
                throw; 
            }
            finally
            {
                stopwatch.Stop();
                await LogRequestAsync(context, requestTime, requestBody, stopwatch.ElapsedMilliseconds);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogRequestAsync(HttpContext context, DateTime requestTime, string requestBody, long durationMs)
        {
            var response = await FormatResponse(context.Response);
            
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var clientName = context.User.Identity?.Name ?? "Anonymous"; 
            var hostName = Dns.GetHostName();

            var logInfo = new
            {
                RequestTime = requestTime,
                ClientIp = clientIp,
                ClientName = clientName,
                HostName = hostName,
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                RequestBody = requestBody,
                ResponseStatusCode = context.Response.StatusCode,
                DurationMs = durationMs
            };

            if (context.Response.StatusCode >= 500)
            {
                Log.Error("Request processed with error: {@LogInfo}", logInfo);
            }
            else
            {
                Log.Information("Request processed successfully: {@LogInfo}", logInfo);
            }
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return text;
        }
    }
}

