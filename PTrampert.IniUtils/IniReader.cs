using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PTrampert.IniUtils;

/// <summary>
/// Reader for INI files.
/// </summary>
/// <param name="options">Options to use when reading an INI file.</param>
public class IniReader(IniOptions options) : IIniReader
{
    private static readonly Regex SectionRegex = new(@"^\[([^\]]+)\]$", RegexOptions.Compiled);
    private static readonly Regex KeyValueRegex = new("^([^=]+)=(.*)$", RegexOptions.Compiled);
    
    private readonly HashSet<string> _currentFiles = new();
    private readonly Stack<string> _fileStack = new();
    
    /// <inheritdoc/>
    public async Task<IniFile> ReadAsync(string filePath, IniSection? rootSection = null)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!_currentFiles.Add(fullPath))
        {
            throw new InvalidOperationException($"Circular include detected for file '{filePath}'");
        }
        _fileStack.Push(fullPath);
        
        try
        {
            await using var stream = File.OpenRead(filePath);
            var result = await ReadAsync(stream, rootSection);
            return result;
        }
        finally
        {
            _currentFiles.Remove(fullPath);
            _fileStack.Pop();
        }
    }
    
    /// <inheritdoc/>
    public async Task<IniFile> ReadAsync(Stream stream, IniSection? rootSection = null)
    {
        var reader = new StreamReader(stream);
        return await ReadAsync(reader, rootSection);
    }
    
    /// <inheritdoc/>
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
                
                if (key == options.IncludesKey)
                {
                    var includePath = value;
                    if (!Path.IsPathRooted(includePath) && _fileStack.Count > 0)
                    {
                        var baseDirectory = Path.GetDirectoryName(_fileStack.Peek());
                        if (!string.IsNullOrEmpty(baseDirectory))
                        {
                            includePath = Path.Combine(baseDirectory, includePath);
                        }
                    }

                    var includedFile = await ReadAsync(includePath);
                    file.Include(includedFile);
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
            throw new IniSyntaxException(lineNumber, line, _fileStack.Count > 0 ? _fileStack.Peek() : null);
        }
        
        return file;
    }
}