using Autodesk.Revit.DB;
using System;

namespace RebarMind.RevitAddin.Services;

/// <summary>
/// Robust transaction wrapper dengan automatic rollback dan error handling (§9 PRD).
/// Memastikan setiap operasi Revit API atomic dan recoverable.
/// </summary>
public class TransactionManager : IDisposable
{
    private readonly Document _doc;
    private readonly Transaction _transaction;
    private bool _committed;
    private bool _disposed;

    public TransactionManager(Document doc, string transactionName)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _transaction = new Transaction(doc, transactionName);
        _transaction.Start();
    }

    public void Commit()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransactionManager));

        _transaction.Commit();
        _committed = true;
    }

    public void RollBack()
    {
        if (_disposed) return;
        _transaction.RollBack();
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (!_committed)
        {
            RollBack();
        }

        _transaction.Dispose();
        _disposed = true;
    }
}
