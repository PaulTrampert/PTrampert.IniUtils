using System;
using System.IO;
using System.Threading.Tasks;

namespace PTrampert.IniUtils;

/// <summary>
/// Interface for IniReader.
/// </summary>
public interface IIniReader
{
    /// <summary>
    /// Read an INI file from the given file path, optionally overriding the root section.
    /// </summary>
    /// <param name="filePath">The location of the ini file.</param>
    /// <param name="rootSection">The starting current section. If not provided, the root section will be "".</param>
    /// <returns>An IniFile object representing the file contents.</returns>
    /// <exception cref="FormatException">Thrown when a line contains invalid INI syntax that cannot be parsed.</exception>
    Task<IniFile> ReadAsync(string filePath, IniSection? rootSection = null);

    /// <summary>
    /// Create a new IniFile from the given Stream, optionally overriding the root section.
    /// </summary>
    /// <param name="stream">The stream to read INI contents from.</param>
    /// <param name="rootSection">The starting current section. If not provided, the root section will be "".</param>
    /// <returns>An IniFile object representing the file contents.</returns>
    /// <exception cref="FormatException">Thrown when a line contains invalid INI syntax that cannot be parsed.</exception>
    Task<IniFile> ReadAsync(Stream stream, IniSection? rootSection = null);

    /// <summary>
    /// Create a new IniFile from the given TextReader, optionally overriding the root section.
    /// </summary>
    /// <param name="reader">The reader to read INI contents from.</param>
    /// <param name="rootSection">The starting current section. If not provided, the root section will be "".</param>
    /// <returns>An IniFile object representing the file contents.</returns>
    /// <exception cref="FormatException">Thrown when a line contains invalid INI syntax that cannot be parsed.</exception>
    Task<IniFile> ReadAsync(TextReader reader, IniSection? rootSection = null);
}