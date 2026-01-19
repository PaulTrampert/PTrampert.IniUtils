namespace PTrampert.IniUtils.Test;

public class IniReaderTests
{
    private IniReader _reader = new();

    [Test]
    public void Read_EmptyFile_ReturnsEmptyIniFile()
    {
        using var sr = new StringReader(string.Empty);
        var result = _reader.Read(sr);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Sections, Is.Not.Null);
        Assert.That(result.Sections.ContainsKey(""), Is.True);
        Assert.That(result.Sections[""].KeyValues, Is.Empty);
    }

    [Test]
    public void Read_SimpleKeyValue_Unquoted()
    {
        using var sr = new StringReader("key=value\n");
        var result = _reader.Read(sr);
        var section = result.Sections[""];
        Assert.That(section.KeyValues.ContainsKey("key"), Is.True);
        var vals = section.KeyValues["key"].ToList();
        Assert.That(vals.Count, Is.EqualTo(1));
        Assert.That(vals[0], Is.EqualTo("value"));
    }

    [Test]
    public void Read_QuotedValue_EscapesHandled()
    {
        using var sr = new StringReader("k=\"a\\nb\\t\"\n");
        var result = _reader.Read(sr);
        var section = result.Sections[""];
        var vals = section.KeyValues["k"].ToList();
        Assert.That(vals.Count, Is.EqualTo(1));
        Assert.That(vals[0], Is.EqualTo("a\nb\t"));
    }

    [Test]
    public void Read_Sections_AndDuplicateKeysAppended()
    {
        var txt = "[sec]\nkey=one\nkey=two\n[other]\na=1\n";
        using var sr = new StringReader(txt);
        var result = _reader.Read(sr);

        Assert.That(result.Sections.ContainsKey("sec"), Is.True);
        var sec = result.Sections["sec"];
        var vals = sec.KeyValues["key"].ToList();
        Assert.That(vals.Count, Is.EqualTo(2));
        Assert.That(vals[0], Is.EqualTo("one"));
        Assert.That(vals[1], Is.EqualTo("two"));

        Assert.That(result.Sections.ContainsKey("other"), Is.True);
        var other = result.Sections["other"];
        Assert.That(other.KeyValues["a"].First(), Is.EqualTo("1"));
    }
}
