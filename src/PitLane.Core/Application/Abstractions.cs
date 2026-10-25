using PitLane.Core.Projects;
using PitLane.Core.Repos;
using PitLane.Core.Runs;

namespace PitLane.Core.Application;

/// <summary>Turns a branch, tag or "default branch" into an exact commit.</summary>
public interface IRefReolver
{
    Task<string> ResolveAsync(RepoRef repo, CancellationToken cancellationToken);
}

/// <summary>Source code for one commit, extracted to a local directory. Immutable once created.</summary>
public sealed record SourceSnapshot(RepoCoordinates Repo, string Sha, string RootPath);

public interface ISourceStore
{
    Task<SourceSnapshot> GetAsync(RepoCoordinates repo, string sha, CancellationToken cancellation);
}

public interface IProjectDiscovery
{
    IReadOnlyList<ProjectInfo> Discover(SourceSnapshot snapshot);
}

/// <summary>A built app, ready to copy into a runtime container.</summary>
public sealed record BuildArtifact(string ArchivePath, string EntryAssembly, string DotNetVersion);

public sealed record BuildOutcome(BuildArtifact? Artifact, long ExitCode, bool TimeOut, bool FromCache)
{
    public bool Succeeded => Artifact is not null;
}


public interface IBuildProvider
{
    Task<BuildOutcome> GetOrBuildAsync(SourceSnapshot source, ProjectInfo project, IRunReporter reporter, CancellationToken cancellationToken);
}

/// <param name="Idle">With <paramref name="TimedOut"/>: stopped because nothing happened, rather than because time ran out.</param>
public sealed record ContainerOutCome(long ExitCode, bool TimedOut, TimeSpan Duration, bool Idle = false);

public interface IAppRunner
{
    /// <summary>The longest an app may run, however busy it is.</summary>
    TimeSpan TimeOut { get; }

    /// <summary>How long an app may go with no input and no output before it is stopped.</summary>
    TimeSpan IdleTimeOut { get; }

    /// <summary>Runs the app attached to <paramref name="console"/>: its output goes there, and keystrokes come from there.</summary>
    Task<ContainerOutCome> RunAsync(BuildArtifact artifact, IReadOnlyList<string> arguments, IRunConsole console, CancellationToken cancellationToken);
}
