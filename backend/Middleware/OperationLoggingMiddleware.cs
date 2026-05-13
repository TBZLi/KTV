using System.Security.Claims;
using System.Text.Json;
using backend.Models;
using backend.Repositories;

namespace backend.Middleware;

public class OperationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OperationLoggingMiddleware> _logger;

    private static readonly Dictionary<string, (string type, string obj)> RouteMap = new()
    {
        ["POST /api/orders"] = ("create", "order"),
        ["POST /api/orders/*/refund"] = ("refund", "order"),
        ["POST /api/orders/*/complete"] = ("complete", "order"),
        ["POST /api/orders/*/cancel"] = ("cancel", "order"),
        ["POST /api/orders/*/restore"] = ("restore", "order"),
        ["DELETE /api/orders/*"] = ("delete", "order"),
        ["POST /api/accounts"] = ("create", "user"),
        ["PUT /api/accounts/*"] = ("update", "user"),
        ["POST /api/accounts/*/recharge"] = ("balance_adjust", "user"),
        ["POST /api/accounts/*/disable"] = ("disable", "user"),
        ["PUT /api/accounts/*/toggle-status"] = ("toggle_status", "user"),
        ["POST /api/songs"] = ("create", "song"),
        ["PUT /api/songs/*"] = ("update", "song"),
        ["DELETE /api/songs/*"] = ("delete", "song"),
        ["PUT /api/rooms/*/status"] = ("update_status", "room"),
        ["POST /api/rooms/*/end-session"] = ("end_session", "room"),
        ["PUT /api/settings"] = ("update", "settings"),
        ["POST /api/settings/admin-account/username"] = ("change_username", "admin"),
        ["POST /api/settings/admin-account/password"] = ("change_password", "admin"),
        ["POST /api/holidays"] = ("create", "holiday"),
        ["DELETE /api/holidays/*"] = ("delete", "holiday"),
    };

    public OperationLoggingMiddleware(RequestDelegate next, ILogger<OperationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Capture the response to only log on success
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;

        // Only log successful write operations (POST/PUT/DELETE with 2xx status)
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300
            && (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "DELETE"))
        {
            try
            {
                var logEntry = BuildLogEntry(context);
                if (logEntry != null)
                {
                    var logRepo = context.RequestServices.GetRequiredService<IOperationLogRepository>();
                    await logRepo.CreateAsync(logEntry);
                }
            }
            catch
            {
                // Silently ignore logging failures (e.g. table doesn't exist yet)
            }
        }
    }

    private OperationLog? BuildLogEntry(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";

        // Try exact match first, then wildcard match
        var key = $"{method} {path}";
        if (!RouteMap.TryGetValue(key, out var mapping))
        {
            // Try matching with wildcard patterns
            foreach (var kvp in RouteMap)
            {
                if (kvp.Key.Contains("*"))
                {
                    var pattern = kvp.Key.Replace("*", "");
                    if (key.StartsWith(pattern) || MatchesWildcard(key, kvp.Key))
                    {
                        mapping = kvp.Value;
                        break;
                    }
                }
            }

            if (mapping == default) return null;
        }

        var username = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";

        // Extract object ID from path
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? objectId = null;
        if (segments.Length >= 3)
        {
            objectId = segments[^1];
            // If the last segment is an action (refund/complete/cancel/restore/disable/status/recharge/password/username), use the second-to-last
            var actions = new[] { "refund", "complete", "cancel", "restore", "disable", "status", "recharge", "password", "username", "end-session", "toggle-status" };
            if (actions.Contains(objectId.ToLower()))
            {
                objectId = segments.Length >= 4 ? segments[^2] : null;
            }
        }

        return new OperationLog
        {
            Username = username,
            OperationType = mapping.type,
            ObjectType = mapping.obj,
            ObjectId = objectId,
            Details = $"{method} {path}"
        };
    }

    private static bool MatchesWildcard(string input, string pattern)
    {
        var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", "[^/]+") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(input, regex);
    }
}
