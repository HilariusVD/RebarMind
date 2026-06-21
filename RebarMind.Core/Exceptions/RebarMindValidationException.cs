namespace RebarMind.Core;

public enum ErrorSeverity
{
    Warning,   // ⚠ Can proceed with caution — shown to user but doesn't block
    Blocker    // ❌ Cannot proceed — must be resolved before Generate
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
