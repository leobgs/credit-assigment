using System.Diagnostics;

namespace PengajuanKredit.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        string method = request.Method;
        string path = request.Path;
        string queryString = request.QueryString.HasValue ? request.QueryString.Value! : string.Empty;

        // Masking query string jika terdapat kemungkinan token/password
        if (queryString.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            queryString.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            queryString = "[MASKED]";
        }

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("HTTP Request Dimulai: {Method} {Path} {QueryString}", method, path, queryString);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            int statusCode = context.Response.StatusCode;
            long elapsedMs = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                _logger.LogError("HTTP Request Selesai dengan Server Error: {Method} {Path} - Status: {StatusCode} ({ElapsedMs} ms)",
                    method, path, statusCode, elapsedMs);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning("HTTP Request Selesai dengan Client Error: {Method} {Path} - Status: {StatusCode} ({ElapsedMs} ms)",
                    method, path, statusCode, elapsedMs);
            }
            else
            {
                _logger.LogInformation("HTTP Request Selesai: {Method} {Path} - Status: {StatusCode} ({ElapsedMs} ms)",
                    method, path, statusCode, elapsedMs);
            }
        }
    }
}
