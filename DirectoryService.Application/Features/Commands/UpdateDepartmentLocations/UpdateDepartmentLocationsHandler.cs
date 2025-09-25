using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models.Departments;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsHandler
{
    private readonly ILocationRepository _locationRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;

    public UpdateDepartmentLocationsHandler(
        ILocationRepository locationRepository,
        IDepartmentRepository departmentRepository,
        IValidator<UpdateDepartmentLocationsCommand> validator)
    {
        _locationRepository = locationRepository;
        _departmentRepository = departmentRepository;
        _validator = validator;
    }

    public async Task<Result<Guid, IEnumerable<Error>>> HandleAsync(
        UpdateDepartmentLocationsCommand cmd,
        CancellationToken ct = default)
    {
        var validationResult = _validator.Validate(cmd);
        if (validationResult.IsValid == false)
            return validationResult.Errors.ToAppErrors().ToList();

        var duplicateLocationIds = cmd.Request.LocationIds
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.First())
            .ToArray();

        if (duplicateLocationIds.Length > 0)
            return Error.Validation($"No duplicate location ids are allowed! [{string.Join(';', duplicateLocationIds)}]").ToSingleErrorArray();

        var getActiveLocationsRes = await _locationRepository.GetLocationsAsync(cmd.Request.LocationIds, ct: ct);
        if (getActiveLocationsRes.IsFailure)
            return getActiveLocationsRes.Error.ToSingleErrorArray();

        if (getActiveLocationsRes.Value.Count() != cmd.Request.LocationIds.Length)
        {
            var activeValidLocationIds = getActiveLocationsRes.Value.Select(x => x.Id);
            var invalidLocations = cmd.Request.LocationIds.Where(x => activeValidLocationIds.Contains(x) == false);
            return Error.Validation($"Some locations are inactive or invalid! ids: [{string.Join(';', invalidLocations)}]").ToSingleErrorArray();
        }

        var department = await _departmentRepository.GetDepartmentAsync(cmd.DepartmentId, ct: ct);
        if (department is null)
            return Errors.General.NotFound(typeof(Department), cmd.DepartmentId).ToSingleErrorArray();

        department.SetLocations(cmd.Request.LocationIds);

        var updateRes = await _departmentRepository.UpdateDepartmentAsync(department, ct);
        if (updateRes.IsFailure)
            return updateRes.Error.ToSingleErrorArray();

        return department.Id;
    }
}