using CSharpFunctionalExtensions;
using DirectoryService.Application.Interfaces;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Validator;
using FluentValidation;

namespace DirectoryService.Application.Features.Commands.UpdateDepartment;

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
            return Error.Validation($"No duplicate location ids are allowed! [{String.Join(';', duplicateLocationIds)}]").ToSingleErrorArray();

        var getActiveLocationsRes = await _locationRepository.GetLocationsAsync(cmd.Request.LocationIds, ct: ct);
        if (getActiveLocationsRes.IsFailure)
            return getActiveLocationsRes.Error.ToSingleErrorArray();

        if (getActiveLocationsRes.Value.Count() != cmd.Request.LocationIds.Length)
        {
            var activeValidLocationIds = getActiveLocationsRes.Value.Select(x => x.Id);
            var invalidLocations = cmd.Request.LocationIds.Where(x => activeValidLocationIds.Contains(x) == false);
            return Error.Validation($"Some locations are inactive or invalid! ids: [{String.Join(';', invalidLocations)}]").ToSingleErrorArray();
        }

        var departmentRes = await _departmentRepository.GetDepartmentAsync(cmd.DepartmentId, ct: ct);
        if (departmentRes.IsFailure)
            return departmentRes.Error.ToSingleErrorArray();

        departmentRes.Value.SetLocations(cmd.Request.LocationIds);

        var updateRes = await _departmentRepository.UpdateDepartmentAsync(departmentRes.Value, ct);
        if (updateRes.IsFailure)
            return updateRes.Error.ToSingleErrorArray();

        return departmentRes.Value.Id;
    }
}