using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using Microsoft.Extensions.Logging;
using System.Data;
using IsolationLevel = System.Data.IsolationLevel;

namespace DirectoryService.Shared.Framework.DbConnection;
public class AppDb
{
    private readonly ILogger<AppDbTransaction> _loggerAppDbTransaction;

    private readonly ILogger<AppDb> _logger;

    public IDbConnection Connection { get; init; }

    private AppDbTransaction _transactionWrapper;

    public IDbTransaction? Transaction { get; private set; }

    public IsolationLevel? TransactionIsolationLevel => Transaction?.IsolationLevel;

    public bool HasOngoingTransaction => Transaction is not null;

    public bool HasIsolationLevel => TransactionIsolationLevel is not null;

    public AppDb(
        ILogger<AppDbTransaction> loggerAppDbTransaction,
        ILogger<AppDb> loggerAppDb,
        IDbConnection connection)
    {
        _loggerAppDbTransaction = loggerAppDbTransaction;
        _logger = loggerAppDb;
        Connection = connection;
    }

    public Result<IDbTransaction,Error> BeginTransaction(IsolationLevel? il = null)
    {
        try
        {
            if (Transaction is null)
            {
                if (il is null)
                    _transactionWrapper = new AppDbTransaction(this, _loggerAppDbTransaction);
                else
                    _transactionWrapper = new AppDbTransaction(this, _loggerAppDbTransaction, (IsolationLevel)il);

                Transaction = _transactionWrapper.Transaction;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transaction beginnign!");
            return Errors.Database.TransactionError();
        }

        _logger.LogTrace("Transaction begun successfully...");
        return Result.Success<IDbTransaction, Error>(Transaction);
    }

    public void OnTransactionDisposed()
    {
        Transaction = null;
    }

    public Result<IDbResultTransaction, Error> OpenTransactionIfNotOngoing()
    {
        if (Transaction is null)
        {
            var openTransactionResult = BeginTransaction();
            if (openTransactionResult.IsFailure)
                return openTransactionResult.Error;

            return _transactionWrapper;
        }
        else
        {
            _logger.LogTrace("Opening nullobject transaction...");
            return new NullObjectDbTransaction(Transaction);
        }
    }
}