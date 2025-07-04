namespace Bwadl.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // More permissive CSP for health UI, stricter for other endpoints
        if (context.Request.Path.StartsWithSegments("/health-ui") || 
            context.Request.Path.StartsWithSegments("/health-ui-resources"))
        {
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:";
        }
        else
        {
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
        }

        await _next(context);
    }
}
