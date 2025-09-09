using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Shared.Framework;

[ApiController]
[Route("api/[controller]")]
public abstract class AppController : ControllerBase
{
    public BadRequestObjectResult BadRequest<T>(T[] error)
    {
        var envelope = Envelope.Error([error]);
        return new BadRequestObjectResult(envelope);
    }
    public override BadRequestObjectResult BadRequest(object? error)
        => BadRequest([error]);

    public override OkObjectResult Ok(object? value)
    {
        var envelope = Envelope.Ok(value);
        return new OkObjectResult(envelope);
    }

    protected Result<Guid, Error> GetCurrentUserId()
    {
        string? userId = HttpContext.User.Claims.FirstOrDefault(u => u.Properties.Values.Contains("sub"))?.Value;
        if (String.IsNullOrWhiteSpace(userId))
            return Error.Failure("Unknown user!", "claim.not.found");
        return new Guid(userId);
    }
}
