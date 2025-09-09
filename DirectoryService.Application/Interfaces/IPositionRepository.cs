using CSharpFunctionalExtensions;
using DirectoryService.Domain.Models;
using DirectoryService.Shared.ErrorClasses;

namespace DirectoryService.Application.Interfaces;
public interface IPositionRepository
{
    Task<UnitResult<Error>> AddPositionAsync(Position position);
    Task<Result<Position, Error>> GetPositionAsync(Guid id);
    Task<UnitResult<Error>> UpdatePositionAsync(Position position);
}