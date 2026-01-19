using System.Collections.Generic;

namespace PTrampert.IniUtils;

/// <summary>
/// Data structure representing a section in an INI file. Name is the section name,
/// and KeyValues contains the key-value pairs within that section. Values are stored
/// in the order that they are read from the file.
/// </summary>
public class IniSection
{
    /// <summary>
    /// The section name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Dictionary of key-value pairs in this section. Each key maps to an enumerable of values,
    /// preserving the order in which they were read from the INI file.
    /// </summary>
    public Dictionary<string, IEnumerable<string>> KeyValues { get; } = new();
}