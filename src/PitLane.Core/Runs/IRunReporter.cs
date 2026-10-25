namespace PitLane.Core.Runs;

/// <summary>How the pipeline and the sandbox talk back to whoever is watching a run.</summary>
public interface IRunReporter
{
    void Status(RunPhase phase, string message);

    void Output(OutputChannel channel, string text);
}

/// <summary>A reporter that can also be typed into: what an interactive run needs from whoever is watching.</summary>
public interface IRunConsole : IRunReporter
{
    /// <summary>The terminal's current size, used when the app's container is created.</summary>
    TerminalSize Size { get; }

    /// <summary>Keystrokes and resizes, in order, until the run completes. Single reader.</summary>
    IAsyncEnumerable<TerminalInput> ReadInputAsync(CancellationToken cancellationToken);
}