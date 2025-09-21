using DirectoryService.Application.Features.Commands.CreatePosition;
using DirectoryService.Contracts.Requests;
using DirectoryService.Shared.Framework;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;
public class PositionController : AppController
{
    [HttpPost()]
    public async Task<IActionResult> CreatePosition(
        [FromBody] CreatePositionRequest req,
        [FromServices] CreatePositionHandler handler,
        CancellationToken ct = default)
    {
        var cmd = new CreatePositionCommand(req);

        var result = await handler.HandleAsync(cmd, ct);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }
}
