using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Interfaces;
public interface IPositionRepository
{
    Task<UnitResult<Error>> AddPositionAsync(Position position, CancellationToken ct = default);

    Task<Result<Position, Error>> GetPositionAsync(Guid? id = null, string name = "", bool active = true, CancellationToken ct = default);

    Task<UnitResult<Error>> SyncPositionWithDepartments (Position position, CancellationToken ct = default);

    Task<UnitResult<Error>> UpdatePositionAsync(Position position, CancellationToken ct = default);
}