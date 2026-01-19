using System;

namespace PTrampert.IniUtils;

/// <summary>
/// Thrown when a syntax error is encountered while parsing an INI file.
/// </summary>
public class IniSyntaxException : Exception
{
    /// <summary>
    /// Create a new IniSyntaxException.
    /// </summary>
    /// <param name="lineNumber">Line number the error occurred at.</param>
    /// <param name="lineContent">The content of the line.</param>
    /// <param name="filePath">The file the error occurred in, if a file.</param>
    public IniSyntaxException(int lineNumber, string lineContent, string? filePath)
        : base($"Syntax error{FileMessageClause(filePath)} at line {lineNumber}: '{lineContent}'")
    {
    }

    private static string FileMessageClause(string? filePath)
    {
        return filePath == null ? string.Empty : $" in '{filePath}'";
    }
}