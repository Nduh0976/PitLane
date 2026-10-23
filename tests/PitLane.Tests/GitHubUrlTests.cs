using PitLane.Core.Repos;

namespace PitLane.Tests;

public class GitHubUrlTests
{
    [Theory]
    [InlineData("owner/repo", "owner", "repo", null)]
    [InlineData("github.com/owner/repo", "owner", "repo", null)]
    [InlineData("https://github.com/owner/repo", "owner", "repo", null)]
    [InlineData("https://www.github.com/owner/repo/", "owner", "repo", null)]
    [InlineData("https://github.com/owner/repo.git", "owner", "repo", null)]
    [InlineData("git@github.com:owner/repo.git", "owner", "repo", null)]
    [InlineData("  https://github.com/owner/repo   ", "owner", "repo", null)]
    [InlineData("https://github.com/owner/repo/tree/main", "owner", "repo", "main")]
    [InlineData("https://github.com/owner/repo/tree/feature/login/src/App", "owner", "repo", "feature/login/src/App")]
    [InlineData("https://github.com/owner/repo/tree/feature%2Flogin", "owner", "repo", "feature/login")]
    [InlineData("https://github.com/owner/repo/commit/0123456789abcdef0123456789abcdef01234567", "owner", "repo", "0123456789abcdef0123456789abcdef01234567")]
    [InlineData("https://www.github.com/owner/repo/issues", "owner", "repo", null)]
    [InlineData("https://www.github.com/Zachary-Patten/dotnet.console_games", "Zachary-Patten", "dotnet.console_games", null)]
    public void Parses_supported_forms(string input, string owner, string name, string? gitRef)
    {
        Assert.True(GitHubUrl.TryParse(input, out var result, out var error), error);
        Assert.Equal(owner, result!.Repo.Owner);
        Assert.Equal(name, result.Repo.Name);
        Assert.Equal(gitRef, result.Ref);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("just-a-word")]
    [InlineData("https://gitlab.com/owner/repo")]
    [InlineData("ftp://github.com/owner/repo")]
    [InlineData("https://github.com/-bad/repo")]
    [InlineData("https://github.com/owner/..")]
    [InlineData("https://github.com/owner/re po")]
    [InlineData("owner/repo/tree/ a/../b")]
    public void Rejects_everything_else(string? input)
    {
        Assert.False(GitHubUrl.TryParse(input, out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void Cache_key_ignores_case_because_GitHub_does()
    {
        Assert.Equal(new RepoCoordinates("Owner", "Repo").CacheKey, new RepoCoordinates("owner", "REPO").CacheKey);
    }
}
