# PTrampert.IniUtils

Lightweight .NET library for reading INI files into a small in-memory model.

PTrampert.IniUtils focuses on a simple, predictable parser with an async API and a few useful options. It exposes an easy-to-use reader (`IniReader`) and small data types (`IniFile`, `IniSection`, `IniOptions`) that make consuming INI data straightforward.

## Features

- Read INI data asynchronously from a `TextReader`, `Stream`, or file path.
- Preserve multiple values for the same key (duplicate keys append values).
- Configurable comment character and option to keep empty values.
- Simple in-memory model: sections (including a root section) with key -> values mapping.
- Small surface area — no external dependencies required by the library itself.
- Automatic includes resolution (e.g. `Include = otherfile.ini`).

## Install

Install from NuGet (package id: `PTrampert.IniUtils`):

```bash
# install the package
dotnet add package PTrampert.IniUtils
```

If you consume the project as a package, use the package ID and version that corresponds to the published NuGet package.

## Quick start / Usage

Basic usage (from code):

```csharp
using PTrampert.IniUtils;

var options = new IniOptions();
var reader = new IniReader(options);

// 1) Read from a TextReader (core overload)
using var sr = new StringReader("key1=value1\nkey2=value2");
var iniFromTextReader = await reader.ReadAsync(sr);

// 2) Read from a Stream
using var fs = File.OpenRead("config.ini");
var iniFromStream = await reader.ReadAsync(fs);

// 3) Read directly from a file path
var iniFromFile = await reader.ReadAsync("path/to/config.ini");

// Working with results
var root = iniFromFile.Sections[""]; // root section stored under empty string
if (root.KeyValues.TryGetValue("key1", out var values))
{
    foreach (var v in values)
    {
        Console.WriteLine(v);
    }
}
```

Optional behaviors controlled by `IniOptions`:

- `CommentCharacter` (char) — change the character that marks a full-line comment (default is ';').
- `KeepEmptyValues` (bool) — if true, keys with empty values are kept; otherwise they are ignored.

You can also pass a custom `IniSection` as the `rootSection` parameter to any `ReadAsync` overload to start with a pre-populated root section.

## Build and test

From the repository root:

```bash
# build
dotnet build

# run tests
dotnet test
```

The test project uses NUnit and validates parsing behavior across the available `ReadAsync` overloads.

## Contributing

Contributions, bug reports, and pull requests are welcome. Please include tests for new behavior and keep public API changes small and well-documented.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
