using PitLane.Core;
using PitLane.Core.Runs;

namespace PitLane.Tests;

public class CommandLineTests
{
    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("   ", new string[0])]
    [InlineData("one two three", new[] { "one", "two", "three" })]
    [InlineData("--name \"Player One\" --level 3", new[] { "--name", "Player One", "--level", "3" })]
    [InlineData("'single $quoted' x", new[] { "single $quoted", "x" })]
    [InlineData("a\\ b c", new[] { "a b", "c" })]
    [InlineData("\"say \\\"hi\\\"\" \"back\\\\slash\"", new[] { "say \"hi\"", "back\\slash" })]
    [InlineData("\"keep \\n as is\"", new[] { "keep \\n as is" })]
    [InlineData("\"\" ''", new[] {"", ""})]
    [InlineData("pre\"mid dle\"post", new[] { "premid dlepost" })]
    public void Splits_like_a_shell_without_expanding_anything(string? text, string[] expected) =>
        Assert.Equal(expected, CommandLine.Split(text));

    [Fact]
    public void Shell_syntax_is_just_text()
    {
        // Nothing here is interpreted: the arguments go into the container's command as an array.
        Assert.Equal(new[] { ";", "rm", "-rf", "/", "&&", "$(whoami)", "`id`", "|", ">out" }, CommandLine.Split("; rm -rf / && $(whoami) `id` | >out"));
    }

    [Theory]
    [InlineData("\"unclosed")]
    [InlineData("'unclosed")]
    [InlineData("bell\a")]
    public void Malformed_arguments_are_rejected(string text)
    {
        var ex = Assert.Throws<PitLaneException>(() => CommandLine.Split(text));
        Assert.Equal(ErrorKind.InvalidInput, ex.Kind);
    }

    [Fact]
    public void Limits_are_enforced()
    {
        Assert.Throws<PitLaneException>(() => CommandLine.Split(new string('x', CommandLine.MaxLength + 1)));
        Assert.Throws<PitLaneException>(() => CommandLine.Split(string.Join(' ', Enumerable.Repeat("a", CommandLine.MaxLength + 1))));
        Assert.Equal(CommandLine.MaxArguments, CommandLine.Split(string.Join(' ', Enumerable.Repeat("a", CommandLine.MaxArguments))).Count);
    }
}
