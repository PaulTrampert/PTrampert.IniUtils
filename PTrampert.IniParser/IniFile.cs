using System.Collections.Generic;

namespace PTrampert.IniParser;

public class IniFile
{
    public Dictionary<string, IniSection> Sections { get; } = new();
}