using DirectoryService.Application.Features.Commands.CreateLocation;
using DirectoryService.Contracts.Requests;
using DirectoryService.Shared.Framework;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;
public class LocationsController : AppController
{
    [HttpPost()]
    public async Task<IActionResult> CreateLocation(
        [FromBody] CreateLocationRequest req,
        [FromServices] CreateLocationHandler handler,
        CancellationToken ct = default)
    {
        var cmd = new CreateLocationCommand(
            req.LocationName,
            req.Address,
            req.Timezone);

        var result = await handler.HandleAsync(cmd, ct);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpGet()]
    public async Task<IActionResult> GetLocations(
        [FromQuery] GetLocationsRequest request,
        [FromServices] GetLocationsHandler handler,
        CancellationToken ct = default)
    {
        var cmd = new GetLocationsCommand(new(request.DepartmentIds, request.SearchName, request.IsActive), request.Pagination);
        var result = await handler.HandleAsync(cmd, ct);
        return Ok(result);
    }
}
