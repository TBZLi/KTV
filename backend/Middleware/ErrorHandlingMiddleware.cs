using System.Net;
using System.Text.Json;

namespace backend.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            // Business exceptions (known error messages) return 400
            var isBusinessError = ex.Message.Contains("余额不足")
                || ex.Message.Contains("不存在")
                || ex.Message.Contains("已被禁用")
                || ex.Message.Contains("已存在")
                || ex.Message.Contains("already exists")
                || ex.Message.Contains("无法")
                || ex.Message.Contains("密码")
                || ex.Message.Contains("不能为空")
                || ex.Message.Contains("不一致")
                || ex.Message.Contains("倍率")
                || ex.Message.Contains("日期");

            context.Response.StatusCode = isBusinessError
                ? (int)HttpStatusCode.BadRequest
                : (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = ex.Message }));
        }
    }
}
