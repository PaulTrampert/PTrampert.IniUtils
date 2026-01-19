using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PTrampert.IniUtils;

/// <summary>
/// Reader for INI files.
/// </summary>
/// <param name="options">Options to use when reading an INI file.</param>
public class IniReader(IniOptions options)
{
    private static readonly Regex SectionRegex = new(@"^\[([^\]]+)\]$", RegexOptions.Compiled);
    private static readonly Regex KeyValueRegex = new("^([^=]+)=(.*)$", RegexOptions.Compiled);

    /// <summary>
    /// Create a new IniFile from the given TextReader, optionally overriding the root section.
    /// </summary>
    /// <param name="reader">The reader to read INI contents from.</param>
    /// <param name="rootSection">The starting current section. If not provided, the root section will be "".</param>
    /// <returns>An IniFile object representing the file contents.</returns>
    /// <exception cref="FormatException">Thrown if a line cannot be read.</exception>
    public async Task<IniFile> ReadAsync(TextReader reader, IniSection? rootSection = null)
    {
        var lineNumber = 0;
        var file = new IniFile();
        var currentSection = rootSection ?? new IniSection { Name = "" };
        file.Sections.Add("", currentSection);
        while (await reader.ReadLineAsync() is { } line)
        {
            lineNumber++;
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
            throw new FormatException($"Syntax error at line {lineNumber}: '{line}'");
        }
        
        return file;
    }
}