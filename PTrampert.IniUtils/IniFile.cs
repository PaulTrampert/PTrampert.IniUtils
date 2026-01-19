using System.Collections.Generic;

namespace PTrampert.IniUtils;

/// <summary>
/// Data structure representing an INI file.
/// </summary>
public class IniFile
{
    /// <summary>
    /// The sections in the INI file, keyed by section name. The top-level (global) section
    /// is represented by an empty string as the key.
    /// </summary>
    public Dictionary<string, IniSection> Sections { get; } = new();
}