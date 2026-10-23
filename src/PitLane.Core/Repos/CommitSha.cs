namespace PitLane.Core.Repos;

public static class CommitSha
{
    public static bool IsFull(string? value) =>
        value is { Length: 40 } && value.All(Uri.IsHexDigit);

    public static string Normalize(string value)
    {
        if (!IsFull(value))
        {
            throw new PitLaneException("A commit SHA must be 40 hexadecimal characterts.", ErrorKind.InvalidInput);
        }

        return value.ToLowerInvariant();
    }

    public static string Abbreviate(string sha) => sha.Length > 7 ? sha[..7] : sha;
}
