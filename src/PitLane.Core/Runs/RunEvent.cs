namespace PitLane.Core.Runs;

public enum RunPhase
{
    Queued,
    Fetching,
    Building,
    Running,
    Succeeded,
    Failed,
    Cancelled,
    TimedOut,
}

public enum OutputChannel
{
    /// <summary>Output from dotnet publish.</summary>
    Build,
    StdOut,
    StdErr,
    /// <summary>Notes from PitLane itself, such as "output truncated."</summary>
    System,
}

public enum RunEventKind
{
    Status,
    Output,
    Completed,
}

/// <summary>
/// One entry in a run's event log. A single flat shape (rather than a class heirarchy) keeps the
/// wire format obvious: the browser switches on <see cref="Kind"/> and reads the fields it needs.
/// </summary>
public sealed record RunEvent(
    long Sequence,
    DateTimeOffset At,
    RunEventKind Kind,
    RunPhase Phase,
    string? Text = null,
    OutputChannel? Channel = null,
    long? ExitCode = null,
    double? ElapsedMs = null);

public static class RunPhaseExtensions
{
    public static bool IsTerminal(this RunPhase phase) =>
        phase is RunPhase.Succeeded or RunPhase.Failed or RunPhase.Cancelled or RunPhase.TimedOut;
}
