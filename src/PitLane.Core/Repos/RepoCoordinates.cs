using System.Text.RegularExpressions;

namespace PitLane.Core.Repos;

/// <summary>An owner/name pair that has already been validated, so it is safe to put in URLs and paths.</summary>
public sealed partial record RepoCoordinates
{
    public RepoCoordinates(string owner, string name)
    {
        if (!IsValidOwner(owner))
        {
            throw new PitLaneException($"'{owner}' isn't a valid GitHub owner name.", ErrorKind.InvalidInput);
        }

        if (!IsValidName(name))
        {
            throw new PitLaneException($"'{name}' isn't a valid GitHub repository name.", ErrorKind.InvalidInput);
        }

        Owner = owner;
        Name = name;
    }

    public string Owner { get; }
    public string Name { get; }

    /// <summary>GitHub names are case-insensitive, so caches key on the lower-cased form.</summary>
    public string CacheKey => $"{Owner.ToLowerInvariant()}/{Name.ToLowerInvariant()}";

    public override string ToString() => $"{Owner}/{Name}";

    public static bool IsValidOwner(string? owner) => owner is not null && OwnerPattern().IsMatch(owner);

    public static bool IsValidName(string? name) =>
        name != null && name is not "." and not ".." && NamePattern().IsMatch(name);

    [GeneratedRegex("^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$")]
    private static partial Regex OwnerPattern();

    [GeneratedRegex("^[A-Za-z0-9._-]{1,100}$")]
    private static partial Regex NamePattern();
}

/// <summary>A repository plus whatever ref the user pointed at (branch, tag, commit) - or null for the default branch.</summary>
public sealed record RepoRef(RepoCoordinates Repo, string? Ref);
