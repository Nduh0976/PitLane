using PitLane.Core.Repos;

namespace PitLane.Core.Runs;

/// <summary>What the user asked to run: a project, identified by its path, at an exact commit.</summary>
public sealed record RunRequest(RepoCoordinates Repo, string Sha, string ProjectPath)
{
    /// <summary>Command-line arguments for the app, already split (see <see cref="CommandLine"/>).</summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];
}
