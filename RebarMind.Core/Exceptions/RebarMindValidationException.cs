namespace RebarMind.Core;

public enum ErrorSeverity
{
    Blocker,   // ❌ Must fix before transaction
    Warning    // ⚠️ Can proceed with caution
}

/// <summary>
/// Thrown when validation fails BEFORE any Revit transaction starts (§7.3).
/// </summary>
public class RebarMindValidationException : Exception
{
    public ErrorSeverity Severity { get; }

    public RebarMindValidationException(string message, ErrorSeverity severity, Exception? inner = null)
        : base(message, inner)
    {
        Severity = severity;
    }
}
