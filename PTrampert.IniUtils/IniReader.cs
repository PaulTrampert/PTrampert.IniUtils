using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PTrampert.IniUtils;

public class IniReader(IniOptions options)
{
    private Regex sectionRegex = new Regex(@"^\[([^\]]+)\]$", RegexOptions.Compiled);
    
    public async Task<IniFile> ReadAsync(TextReader reader, IniSection? rootSection = null)
    {
        var file = new IniFile();
        var currentSection = rootSection ?? new IniSection { Name = "" };
        file.Sections.Add("", currentSection);
        while (await reader.ReadLineAsync() is { } line)
        {
            line = StripComment(line);
            line = line.Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            
            var sectionMatch = sectionRegex.Match(line);
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

            if (line.Contains('='))
            {
                var keyValue = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();

                if (keyValue.Length >= 1)
                {
                    var key = keyValue[0];
                    var value = keyValue.Length == 2 ? keyValue[1] : null;

                    if (!currentSection.KeyValues.TryGetValue(key, out var values))
                    {
                        values = Array.Empty<string>();
                    }

                    if (value != null)
                    {
                        currentSection.KeyValues[key] = values.Append(value);
                    }
                }

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