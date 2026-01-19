using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PTrampert.IniUtils;

public class IniReader(IniOptions options)
{
    private static readonly Regex SectionRegex = new(@"^\[([^\]]+)\]$", RegexOptions.Compiled);
    private static readonly Regex KeyValueRegex = new("^([^=]+)=(.*)$", RegexOptions.Compiled);
    
    public async Task<IniFile> ReadAsync(TextReader reader, IniSection? rootSection = null)
    {
        var file = new IniFile();
        var currentSection = rootSection ?? new IniSection { Name = "" };
        file.Sections.Add("", currentSection);
        while (await reader.ReadLineAsync() is { } line)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(options.CommentCharacter))
            {
                continue;
            }
            
            var sectionMatch = SectionRegex.Match(line);
            if (sectionMatch.Success)
            {
                var sectionName = sectionMatch.Groups[1].Value.Trim();
                if (!file.Sections.TryGetValue(sectionName, out var section))
                {
                    section = new IniSection { Name = sectionName };
                    file.Sections[sectionName] = section;
                }
                currentSection = section;
                continue;
            }

            var keyValueMatch = KeyValueRegex.Match(line);
            if (keyValueMatch.Success)
            {
                var key = keyValueMatch.Groups[1].Value.Trim();
                var value = keyValueMatch.Groups[2].Value.Trim();

                if (string.IsNullOrEmpty(value) && !options.KeepEmptyValues)
                {
                    continue;
                }
                
                if (!currentSection.KeyValues.TryGetValue(key, out var values))
                {
                    values = Array.Empty<string>();
                    currentSection.KeyValues.Add(key, values);
                }

                currentSection.KeyValues[key] = values.Append(value);
                
                continue;
            }
            throw new FormatException($"Invalid line in INI file: '{line}'");
        }
        
        return file;
    }

    private string StripComment(string line)
    {
        var index = line.IndexOf(options.CommentCharacter);
        return index >= 0 ? line[..index] : line;
    }
}