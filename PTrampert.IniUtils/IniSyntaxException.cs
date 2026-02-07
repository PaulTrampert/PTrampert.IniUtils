using System;

namespace PTrampert.IniUtils;

/// <summary>
/// Thrown when a syntax error is encountered while parsing an INI file.
/// </summary>
public class IniSyntaxException : Exception
{
    /// <summary>
    /// The line number the syntax exception occurred at.
    /// </summary>
    public int LineNumber { get; }
    
    /// <summary>
    /// The content of the line that caused the syntax exception.
    /// </summary>
    public string LineContent { get; }
    
    /// <summary>
    /// The file path the syntax exception occurred in, if applicable.
    /// </summary>
    public string? FilePath { get; }
    
    /// <summary>
    /// Create a new IniSyntaxException with default values.
    /// </summary>
    public IniSyntaxException() : this(0, string.Empty, null)
    {
    }
    
    /// <summary>
    /// Create a new IniSyntaxException with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public IniSyntaxException(string message) : this(message, 0, string.Empty, null)
    {
    }
    
    /// <summary>
    /// Create a new IniSyntaxException with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public IniSyntaxException(string message, Exception innerException) 
        : this(message, innerException, 0, string.Empty, null)
    {
    }
    
    /// <summary>
    /// Create a new IniSyntaxException with a custom message and diagnostic information.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="lineNumber">Line number the error occurred at.</param>
    /// <param name="lineContent">The content of the line.</param>
    /// <param name="filePath">The file the error occurred in, if a file.</param>
    public IniSyntaxException(string message, int lineNumber, string lineContent, string? filePath)
        : base(message)
    {
        LineNumber = lineNumber;
        LineContent = lineContent;
        FilePath = filePath;
    }
    
    /// <summary>
    /// Create a new IniSyntaxException with a custom message, inner exception, and diagnostic information.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="lineNumber">Line number the error occurred at.</param>
    /// <param name="lineContent">The content of the line.</param>
    /// <param name="filePath">The file the error occurred in, if a file.</param>
    public IniSyntaxException(string message, Exception innerException, int lineNumber, string lineContent, string? filePath)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
        LineContent = lineContent;
        FilePath = filePath;
    }
    
    /// <summary>
    /// Create a new IniSyntaxException.
    /// </summary>
    /// <param name="lineNumber">Line number the error occurred at.</param>
    /// <param name="lineContent">The content of the line.</param>
    /// <param name="filePath">The file the error occurred in, if a file.</param>
    public IniSyntaxException(int lineNumber, string lineContent, string? filePath)
        : base($"Syntax error{FileMessageClause(filePath)} at line {lineNumber}: '{lineContent}'")
    {
        LineNumber = lineNumber;
        LineContent = lineContent;
        FilePath = filePath;
    }

    private static string FileMessageClause(string? filePath)
    {
        return filePath == null ? string.Empty : $" in '{filePath}'";
    }
}