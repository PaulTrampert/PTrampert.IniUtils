using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;

namespace PTrampert.IniParser;

internal class IniTreeVisitor : IniGrammarBaseVisitor<IniFile>
{
    private IniFile _iniFile = null!;
    private IniSection _currentSection = null!;

    private readonly Regex _escapeRegex = new Regex("\\\\[nrt\\\\\\\"']", RegexOptions.Compiled);
    private readonly Dictionary<string, string> _escapeSequences = new()
    {
        { "\\n", "\n" },
        { "\\r", "\r" },
        { "\\t", "\t" },
        { "\\\\", "\\" },
        { "\\\"", "\"" },
        { "\\'", "'" }
    };
        
    public override IniFile VisitIniFile(IniGrammar.IniFileContext context)
    {
        _iniFile = new IniFile();
        _currentSection = new IniSection();
        _iniFile.Sections.Add(_currentSection.Name, _currentSection);
        foreach (var line in context.iniLine())
        {
            Visit(line);
        }

        return _iniFile;
    }

    public override IniFile VisitSectionName(IniGrammar.SectionNameContext context)
    {
        var sectionName = context.GetText();
            
        if (!_iniFile.Sections.TryGetValue(sectionName, out _currentSection))
        {
            _currentSection = new IniSection { Name = sectionName };
            _iniFile.Sections.Add(sectionName, _currentSection);
        }
            
        return _iniFile;
    }


    public override IniFile VisitKeyValuePair(IniGrammar.KeyValuePairContext context)
    {
        var key = ReadString(context.key().@string());
        var value = string.Join(" ", context.value().@string().Select(ReadString));
            
        if (!_currentSection.KeyValues.TryGetValue(key, out var values))
        {
            values = new List<string>();
            _currentSection.KeyValues.Add(key, values);
        }

        _currentSection.KeyValues[key] = values.Append(value);

        return _iniFile;
    }

    private string ReadString(IniGrammar.StringContext context)
    {
        var text = context.GetText();
        if (context.DQUOTED_STRING() != null || context.SQUOTED_STRING() != null)
        {
            text = UnescapeString(text[1..^1]);
        }
        return text;
    }

    private string UnescapeString(string str)
    {
        return _escapeRegex.Replace(str, match => _escapeSequences.TryGetValue(match.Value, out var result) ? result : match.Value);
    }
}