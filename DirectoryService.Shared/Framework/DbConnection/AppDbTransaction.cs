using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using DirectoryService.Shared.Framework.DbConnection;
using Microsoft.Extensions.Logging;
using System.Data;
using IsolationLevel = System.Data.IsolationLevel;

namespace DirectoryService.Shared.Framework;

public class AppDbTransaction : IDbResultTransaction, IDisposable
{
    private readonly AppDb _connectionWrapper;
    private readonly ILogger<AppDbTransaction> _logger;

    public IDbTransaction Transaction { get; init; }

    public IDbConnection? Connection => Transaction.Connection;

    public IsolationLevel IsolationLevel => Transaction.IsolationLevel;

    public AppDbTransaction(AppDb connection, ILogger<AppDbTransaction> logger, IsolationLevel? Il = null)
    {
        _logger = logger;
        _connectionWrapper = connection;

        try
        {
            if (connection.Connection.State == ConnectionState.Closed)
            connection.Connection.Open();

            if (Il is null)
                Transaction = connection.Connection.BeginTransaction();
            else
                Transaction = connection.Connection.BeginTransaction((IsolationLevel)Il);
        }
        catch (Exception ex) // handle it on creation site
        {
            throw;
        }
    }

    public void Commit()
    {
        Transaction.Commit();
    }

    public UnitResult<Error> TryCommit()
    {
        try
        {
            Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction error during commit!");
            return Errors.Database.TransactionError();
        }

        _logger.LogTrace("Transaction commit success");

        return Result.Success<Error>();
    }


    public void Rollback()
    {
        Transaction.Rollback();
    }

    public UnitResult<Error> TryRollback()
    {
        try
        {
            Rollback();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction error during rollback!");
            return Errors.Database.TransactionError();
        }

        _logger.LogTrace("Transaction rollback success");

        return Result.Success<Error>();
    }


    public void Dispose()
    {
        Transaction.Dispose();
        _connectionWrapper.OnTransactionDisposed();
    }
}
