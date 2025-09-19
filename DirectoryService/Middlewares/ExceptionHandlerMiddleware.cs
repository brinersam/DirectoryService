using DirectoryService.Shared;
using DirectoryService.Shared.ErrorClasses;
using System.Net;

namespace DirectoryService.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger)
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
            _logger.LogError("Exception was caught!: {error} {stacktrace}", ex.Message, ex.StackTrace);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var err = Error.Failure("exception", ex.Message);

            await context.Response.WriteAsJsonAsync(Envelope.Error([err]));
        }
    }
}
