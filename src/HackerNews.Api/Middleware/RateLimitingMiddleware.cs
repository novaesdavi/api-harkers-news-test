using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HackerNews.Api.Middleware
{
    // Simple in-memory rate limiting middleware.
    // Not suitable for production if you need strict guarantees or distributed limits.
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _maxRequests;
        private readonly TimeSpan _window;
        private static readonly ConcurrentDictionary<string, Window> _counters = new();
        private const int _defaultMaxRequests = 1000;
        private const int _defaultWindowSeconds = 10;

        private class Window { public int Count; public DateTime Start; }

        public RateLimitingMiddleware(RequestDelegate next, int maxRequests = _defaultMaxRequests, TimeSpan? window = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _maxRequests = maxRequests;
            _window = window ?? TimeSpan.FromSeconds(_defaultWindowSeconds);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string key = ResolveClientKey(context);
            var now = DateTime.UtcNow;
            var window = _counters.GetOrAdd(key, _ => new Window { Count = 0, Start = now });

            lock (window)
            {
                if ((now - window.Start) > _window)
                {
                    window.Start = now; 
                    window.Count = 0;
                }

                Console.WriteLine($"Client {key}: {window.Count + 1}/{_maxRequests} requests in current window.");

                window.Count++;
                if (window.Count > _maxRequests)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = ((int)_window.TotalSeconds).ToString();
                    return;
                }
            }

            await _next(context);
        }

        private static string ResolveClientKey(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues vals) && vals.Count > 0)
            {
                return vals[0].Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseSimpleRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
