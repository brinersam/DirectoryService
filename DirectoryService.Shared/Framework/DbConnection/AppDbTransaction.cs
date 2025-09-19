using System.Data;
using IsolationLevel = System.Data.IsolationLevel;

namespace DirectoryService.Shared.Framework;

public class AppDbTransaction : IDbTransaction, IDisposable
{
    private readonly AppDb _connectionWrapper;

    public IDbTransaction Transaction { get; init; }

    public IDbConnection? Connection => Transaction.Connection;

    public IsolationLevel IsolationLevel => Transaction.IsolationLevel;

    public AppDbTransaction(AppDb connection, IsolationLevel? Il = null)
    {
        _connectionWrapper = connection;
        if (connection.Connection.State == ConnectionState.Closed)
            connection.Connection.Open();

        if (Il is null)
            Transaction = connection.Connection.BeginTransaction();
        else
            Transaction = connection.Connection.BeginTransaction((IsolationLevel)Il);
    }

    public void Commit()
    {
        Transaction.Commit();
    }

    public void Rollback()
    {
        Transaction.Rollback();
    }

    public void Dispose()
    {
        Transaction.Dispose();
        _connectionWrapper.OnTransactionDisposed();
    }
}
