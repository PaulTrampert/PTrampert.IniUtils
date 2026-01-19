namespace PTrampert.IniUtils;

/// <summary>
/// Options for reading INI files.
/// </summary>
public class IniOptions
{
    /// <summary>
    /// The character that indicates the start of a comment line. Default is ';'.
    /// </summary>
    public char CommentCharacter { get; set; } = ';';

    /// <summary>
    /// If true, keys with empty values will be kept in the output. Specifically,
    /// lines like "key=" will result in the key "key" with an empty string as its value.
    /// </summary>
    public bool KeepEmptyValues { get; set; }
}