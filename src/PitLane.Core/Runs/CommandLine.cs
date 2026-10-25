using System.Text;

namespace PitLane.Core.Runs;

/// <summary>
/// Splits an argument string the way a POSIX shell would, minus everything dangerous: whitespace
/// separates arguments, '...' is literal, "..." honours \" and \\, and a backslash outside quotes escapes
/// the next character. There's no expansion of any kind, and the result goes into the container's
/// command as an array, never through a shell, so nothing here can inject a command.
/// </summary>
public static class CommandLine
{
    public const int MaxLength = 2000;
    public const int MaxArguments = 64;

    public static IReadOnlyList<string> Split(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        if (text.Length > MaxLength)
        {
            throw new PitLaneException($"Arguments can be at most {MaxLength} characters long.", ErrorKind.InvalidInput);
        }

        var arguments = new List<string>();
        var current = new StringBuilder();
        var inArgument = false;
        char? quote = null;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsControl(c) && c is not ('\t' or '\n' or '\r'))
            {
                throw new PitLaneException("Arguments cannot contain control characters", ErrorKind.InvalidInput);
            }

            if (quote == '\'')
            {
                if (c == '\'')
                {
                    quote = null;
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (quote == '"')
            {
                if (c == '"')
                {
                    quote = null;
                }
                else if (c == '\\' && i + 1 < text.Length && text[i + 1] is '"' or '\\')
                {
                    current.Append(text[++i]);
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (char.IsWhiteSpace(c))
            {
                if (inArgument)
                { 
                    arguments.Add(current.ToString());
                }

                current.Clear();
                inArgument = false;
            }
            else
            {
                inArgument = true;

                if (c is '\'' or '"')
                {
                    quote = c;
                }
                else if (c == '\\' && i + 1 < text.Length)
                {
                    current.Append(text[++i]);
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        if (quote is not null)
        {
            throw new PitLaneException($"The arguments have an unclosed {quote} quote.", ErrorKind.InvalidInput);
        }

        if (inArgument)
        {
            arguments.Add(current.ToString());
        }

        if (arguments.Count > MaxArguments)
        {
            throw new PitLaneException($"That's {arguments.Count} arguments. The limit is {MaxArguments}.", ErrorKind.InvalidInput);
        }

        return arguments;
    }
}
