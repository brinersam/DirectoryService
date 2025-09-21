using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using System.Data;

namespace DirectoryService.Shared.Framework.DbConnection;
public interface IDbResultTransaction : IDbTransaction
{
    public UnitResult<Error> TryCommit();

    public UnitResult<Error> TryRollback();
}
