using System.Diagnostics.CodeAnalysis;

namespace PitLane.Core.Repos;

/// <summary>
/// Turns whatever the user pasted into a <see cref="RepoRef"/>. Accepts:
/// owner/repo, github.com/owner/repo, https://github.com/owner/repo(.git),
/// .../tree/{branch-or-tag}, ../commit/{sha} and git@github.com:owner/repo.git.
/// </summary>
public static class GitHubUrl
{
    private const int MaxRefLength = 255;

    public static bool TryParse(string? input, [NotNullWhen(true)] out RepoRef? result, out string error)
    {
        result = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Paste a GitHub repository URL, like https://github.com/owner/repo.";

            return false;
        }

        var text = input.Trim();

        if (text.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase))
        {
            text = text["git@github.com:".Length..];
        }
        else
        {
            if (!text.Contains("://", StringComparison.Ordinal)
                && (text.StartsWith("github.com/", StringComparison.OrdinalIgnoreCase)
                    || text.StartsWith("www.github.com/", StringComparison.OrdinalIgnoreCase)))
            {
                text = "https://" + text;
            }

            if (text.Contains("://", StringComparison.Ordinal))
            {
                if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
                    !(uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
                        uri.Host.Equals("www.github.com", StringComparison.OrdinalIgnoreCase)))
                {
                    error = "Only repositories on github.com are supported.";
                    return false;
                }

                text = Uri.UnescapeDataString(uri.AbsolutePath);
            }
        }

        var segments = text.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length < 2)
        {
            error = "That doesn't look like a repository. Use the form owner/repo.";
            return false;
        }

        var owner = segments[0];
        var name = segments[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? segments[1][..^4] : segments[1];

        if (!RepoCoordinates.IsValidOwner(owner) || !RepoCoordinates.IsValidName(name))
        {
            error = $"'{owner}/{name}' isn't a valid GitHub repository name.";
            return false;
        }

        string? gitRef = null;
        if(segments.Length > 3 && segments[2] is "tree" or "commit" or "blob")
        {
            // Branch names can contain slashes (feature/login), so keep everything after /tree/.
            // The ref resolver works out where the branch name ends and any sub-path begins.
            gitRef = string.Join('/', segments[3..]);

            if (gitRef.Length > MaxRefLength || gitRef.Any(char.IsControl) || gitRef.Contains("..", StringComparison.Ordinal))
            {
                error = "That branch or tag name isn't valid.";
                return false;
            }
        }

        result = new RepoRef(new RepoCoordinates(owner, name), gitRef);
        return true;
    }
}
