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
        file.Sections.Add(currentSection.Name, currentSection);
        while (await reader.ReadLineAsync() is { } line)
        {
            lineNumber++;
            line = line.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(options.CommentCharacter))
            {
                continue;
            }

            // process the trimmed, non-empty, non-comment line using a helper
            currentSection = await ProcessLineAsync(file, currentSection, line, lineNumber);
        }

        return file;
    }

    // Helper: process a key + value (handles KeepEmptyValues, IncludesKey resolution, and adding values)
    private async Task ProcessKeyValueAsync(IniFile file, IniSection currentSection, string key, string value)
    {
        // value is expected to be trimmed by caller
        if (string.IsNullOrEmpty(value) && !options.KeepEmptyValues)
        {
            return; // skip storing empty values
        }

        if (key == options.IncludesKey)
        {
            // Skip empty include paths - they are not valid
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var includePath = value;
            if (!Path.IsPathRooted(includePath) && _fileStack.Count > 0)
            {
                var baseDirectory = Path.GetDirectoryName(_fileStack.Peek());
                if (!string.IsNullOrEmpty(baseDirectory))
                {
                    includePath = Path.Combine(baseDirectory, includePath);
                }
            }

            var includedFile = await ReadAsync(includePath, currentSection);
            file.Include(includedFile);
            return;
        }

        if (!currentSection.KeyValues.TryGetValue(key, out var values))
        {
            values = Array.Empty<string>();
            currentSection.KeyValues.Add(key, values);
        }

        currentSection.KeyValues[key] = values.Append(value);
    }

    // Helper: get existing section or create and add it
    private IniSection GetOrAddSection(IniFile file, string sectionName)
    {
        if (!file.Sections.TryGetValue(sectionName, out var section))
        {
            section = new IniSection { Name = sectionName };
            file.Sections[sectionName] = section;
        }

        return section;
    }

    // Helper: process a non-empty, non-comment line and return the (possibly updated) current section
    private async Task<IniSection> ProcessLineAsync(IniFile file, IniSection currentSection, string line,
        int lineNumber)
    {
        var sectionMatch = SectionRegex.Match(line);
        if (sectionMatch.Success)
        {
            var sectionName = sectionMatch.Groups[1].Value.Trim();
            return GetOrAddSection(file, sectionName);
        }

        var keyValueMatch = KeyValueRegex.Match(line);
        if (keyValueMatch.Success)
        {
            var key = keyValueMatch.Groups[1].Value.Trim();
            var value = keyValueMatch.Groups[2].Value.Trim();

            await ProcessKeyValueAsync(file, currentSection, key, value);
            return currentSection;
        }

        if (options.AllowKeyWithoutEquals && !line.Contains('='))
        {
            var key = line;
            var value = string.Empty;

            await ProcessKeyValueAsync(file, currentSection, key, value);
            return currentSection;
        }

        throw new IniSyntaxException(lineNumber, line, _fileStack.Count > 0 ? _fileStack.Peek() : null);
    }
}