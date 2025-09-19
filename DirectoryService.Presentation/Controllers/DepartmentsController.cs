using DirectoryService.Application.Features.Commands.CreateDepartment;
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
}
