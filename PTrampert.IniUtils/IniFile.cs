using System.Collections.Generic;

namespace PTrampert.IniUtils;

public class IniFile
{
    public Dictionary<string, IniSection> Sections { get; } = new();
}