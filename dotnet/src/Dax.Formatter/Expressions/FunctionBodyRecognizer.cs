namespace Dax.Formatter.Expressions;

/// <summary>
/// Tells the body of a user-defined function from any other expression: after spaces and comments, a parameter
/// list in balanced parentheses followed by <c>=&gt;</c>, an arrow that DAX has only in function definitions.
/// </summary>
internal static class FunctionBodyRecognizer
{
    public static bool IsFunctionBody(string text)
    {
        var scanner = new DaxTextScanner(text, 0);
        if (!ReadSignificant(scanner, '('))
        {
            return false;
        }

        // A default value of a parameter can hold nested parentheses, strings and names.
        var depth = 1;
        while (depth > 0)
        {
            var kind = scanner.Read();
            if (kind == DaxTokenKind.End)
            {
                return false;
            }

            if (kind == DaxTokenKind.Other && scanner.TokenCharacter == '(')
            {
                depth++;
            }
            else if (kind == DaxTokenKind.Other && scanner.TokenCharacter == ')')
            {
                depth--;
            }
        }

        if (!ReadSignificant(scanner, '='))
        {
            return false;
        }

        return scanner.Read() == DaxTokenKind.Other && scanner.TokenCharacter == '>';
    }

    // Reads past spaces, line breaks and comments, then says whether the next character is the expected one.
    private static bool ReadSignificant(DaxTextScanner scanner, char expected)
    {
        while (true)
        {
            var kind = scanner.Read();
            if (kind is DaxTokenKind.LineBreak or DaxTokenKind.LineComment or DaxTokenKind.BlockComment)
            {
                continue;
            }

            if (kind != DaxTokenKind.Other)
            {
                return false;
            }

            if (!char.IsWhiteSpace(scanner.TokenCharacter))
            {
                return scanner.TokenCharacter == expected;
            }
        }
    }
}
