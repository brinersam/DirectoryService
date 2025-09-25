using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using System.Data;
using IsolationLevel = System.Data.IsolationLevel;

namespace DirectoryService.Shared.Framework.DbConnection;

public class NullObjectDbTransaction : IDbResultTransaction, IDisposable
{
    private readonly IDbTransaction _transaction;

    public IDbConnection? Connection => _transaction.Connection;

    public IsolationLevel IsolationLevel => _transaction.IsolationLevel;

    public NullObjectDbTransaction(IDbTransaction original)
    {
        _transaction = original;
    }

    public void Commit()
    { }

    public void Rollback()
    { }

    public void Dispose()
    { }

    public UnitResult<Error> TryCommit()
    {
        return Result.Success<Error>();
    }

    public UnitResult<Error> TryRollback() 
    {
        return Result.Success<Error>();
    }
}