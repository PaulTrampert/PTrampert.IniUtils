using System.Collections.Generic;

namespace PTrampert.IniParser;

public class IniSection
{
    public string Name { get; set; }
    
    public Dictionary<string, IEnumerable<string>> KeyValues { get; } = new();
}