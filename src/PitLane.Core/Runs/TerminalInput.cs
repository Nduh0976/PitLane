namespace PitLane.Core.Runs;

/// <summary>The browser terminal's size in character cells. Clamped, because it comes from the browser.</summary>
public readonly record struct TerminalSize
{
    public static readonly TerminalSize Default = new(100, 30);

    public TerminalSize(int columns, int rows)
    {
        Columns = Math.Clamp(columns, 20, 500);
        Rows = Math.Clamp(rows, 5, 200);
    }

    public int Columns { get; }
    public int Rows { get; }
}

/// <summary>
/// Something the person at the terminal did: typed (or pasted) <see cref="Text"/>, or resized the
/// terminal to <see cref="Size"/>. One flat shape, like <see cref="RunEvent"/>.
/// </summary>
public sealed record TerminalInput(string? Text = null, TerminalSize? Size = null)
{
    public static TerminalInput Keys(string text) => new(Text: text);

    public static TerminalInput Resized(TerminalSize size) => new(Size: size);
}
