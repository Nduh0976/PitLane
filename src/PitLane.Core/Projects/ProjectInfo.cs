namespace PitLane.Core.Projects;

/// <summary>A project file found in a repository, and whether PitLane can run it.</summary>
public sealed record ProjectInfo
{
    /// <summary>Path relative to the repository root, always with forward slashes. This is the project's identity.</summary>
    public required string Path { get; init; }

    public required string Name { get; init; }
    public required string AssemblyName { get; init; }
    public required string Language { get; init; }
    public string? Sdk { get; init; }
    public string? OutputType { get; init; }
    public IReadOnlyList<string> TargetFrameworks { get; init; } = [];

    ///<summary>The framework PitLane will build, e.g. net10.0. Null when not runnable.</summary>
    public string? SelectedFramework { get; init; }

    ///<summary>The .NET version that picks the container image tag, e.g. 10.0. Null when not runnable.</summary>
    public string? DotNetVersion { get; init; }

    public bool Runnable { get; init; }

    ///<summary>Why the project can't be run, phrased for the user. Null when runnable.</summary>
    public string? Reason { get; init; }
}
