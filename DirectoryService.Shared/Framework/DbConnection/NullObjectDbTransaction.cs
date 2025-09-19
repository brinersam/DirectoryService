using System.Data;
using IsolationLevel = System.Data.IsolationLevel;

namespace DirectoryService.Shared.Framework;

public class NullObjectDbTransaction : IDbTransaction, IDisposable
{
    private readonly IDbTransaction Transaction;

    public IDbConnection? Connection => Transaction.Connection;

    public IsolationLevel IsolationLevel => Transaction.IsolationLevel;

    public NullObjectDbTransaction(IDbTransaction original)
    {
        Transaction = original;
    }

    public void Commit()
    { }

    public void Rollback()
    { }

    public void Dispose()
    { }
}