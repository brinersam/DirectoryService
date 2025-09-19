using System.Data;
using IsolationLevel = System.Data.IsolationLevel;

namespace DirectoryService.Shared.Framework;
public class AppDb
{
    public IDbConnection Connection { get; init; }

    private AppDbTransaction _transactionWrapper;

    public IDbTransaction? Transaction { get; private set; }

    public IsolationLevel? TransactionIsolationLevel => Transaction?.IsolationLevel;

    public bool HasOngoingTransaction => Transaction is not null;

    public bool HasIsolationLevel => TransactionIsolationLevel is not null;

    public AppDb(IDbConnection connection)
    {
        Connection = connection;
    }

    public IDbTransaction BeginTransaction()
    {
        if (Transaction is null)
        {
            _transactionWrapper = new AppDbTransaction(this);
            Transaction = _transactionWrapper.Transaction;
        }

        return Transaction;
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        if (Transaction is null)
        {
            _transactionWrapper = new AppDbTransaction(this, il);
            Transaction = _transactionWrapper.Transaction;
        }

        return Transaction;
    }

    public void OnTransactionDisposed()
    {
        Transaction = null;
    }

    public IDbTransaction OpenTransactionIfNotOngoing()
    {
        if (Transaction is null)
        {
            BeginTransaction();
            return _transactionWrapper;
        }
        else
        {
            return new NullObjectDbTransaction(Transaction);
        }
    }
}