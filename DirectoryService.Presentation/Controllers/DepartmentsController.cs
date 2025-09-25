using DirectoryService.Application.Features.Commands.CreateDepartment;
using DirectoryService.Application.Features.Commands.UpdateDepartment;
using DirectoryService.Contracts.Requests;
using DirectoryService.Shared.Framework;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;
public class DepartmentsController : AppController
{
    [HttpPost()]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] CreateDepartmentRequest req,
        [FromServices] CreateDepartmentHandler handler,
        CancellationToken ct = default)
    {
        var cmd = new CreateDepartmentCommand(req);

        var result = await handler.HandleAsync(cmd, ct);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpPut("{departmentId:Guid}/locations")]
    public async Task<IActionResult> UpdateDepartmentLocations(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentLocationsRequest req,
        [FromServices] UpdateDepartmentLocationsHandler handler,
        CancellationToken ct = default)
    {
        var cmd = new UpdateDepartmentLocationsCommand(departmentId, req);

        var result = await handler.HandleAsync(cmd, ct);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpPut("{departmentId:Guid}/parent")]
    public async Task<IActionResult> SetDepartmentParent(
        [FromRoute] Guid departmentId,
        [FromBody] SetDepartmentParentRequest req,
        [FromServices] SetDepartmentParentHandler handler,
        CancellationToken ct = default)
    {
        var cmd = new SetDepartmentParentCommand(departmentId, req);

        var result = await handler.HandleAsync(cmd, ct);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }
}
