namespace PitLane.Core;

/// <summary>What kind of failure this is, so the web laye can pick a status code without knowing the cause.</summary>
public enum ErrorKind
{
    InvalidInput,
    NotFound,
    Busy,
    Upstream,
    Unavailable,
}

/// <summary>
/// An expected failure with a message that is safe and useful to show the user.
/// Anything that is not a PitLane Exception is treated as a bug.
/// </summary>
public class PitLaneException(string message, ErrorKind kind, Exception? inner = null) : Exception(message, inner)
{
    public ErrorKind Kind { get; } = kind;
}
