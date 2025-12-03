using System.Diagnostics;

namespace EcomShopping.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private static int _requestCounter = 0;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Interlocked.Increment(ref _requestCounter);
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        
        _logger.LogInformation("[Request #{RequestId}] >>> Incoming: {Method} {Path}", requestId, requestMethod, requestPath);
        
        try
        {
            _logger.LogInformation("[Request #{RequestId}] Calling next middleware...", requestId);
            await _next(context);
            stopwatch.Stop();
            
            _logger.LogInformation("[Request #{RequestId}] <<< Completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms", 
                requestId, requestMethod, requestPath, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[Request #{RequestId}] !!! Failed: {Method} {Path} - Duration: {Duration}ms", 
                requestId, requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
