using System.IO;
using System.Text;

namespace PTrampert.IniUtils.Test;

[TestFixture]
public class IniReaderTests
{
    private IniOptions _defaultOptions;
    
    [SetUp]
    public void SetUp()
    {
        _defaultOptions = new IniOptions();
    }
    
    [Test]
    public async Task ReadAsync_EmptyFile_ReturnsEmptyIniFileWithRootSection()
    {
        // Arrange
        var reader = new StringReader("");
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Sections, Has.Count.EqualTo(1));
        Assert.That(result.Sections.ContainsKey(""), Is.True);
    }
    
    [Test]
    public async Task ReadAsync_SingleSection_ParsesSectionName()
    {
        // Arrange
        var content = "[Section1]";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result.Sections, Has.Count.EqualTo(2));
        Assert.That(result.Sections.ContainsKey("Section1"), Is.True);
        Assert.That(result.Sections["Section1"].Name, Is.EqualTo("Section1"));
    }
    
    [Test]
    public async Task ReadAsync_MultipleSections_ParsesAllSections()
    {
        // Arrange
        var content = @"[Section1]
[Section2]
[Section3]";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result.Sections, Has.Count.EqualTo(4)); // root + 3 sections
        Assert.That(result.Sections.ContainsKey("Section1"), Is.True);
        Assert.That(result.Sections.ContainsKey("Section2"), Is.True);
        Assert.That(result.Sections.ContainsKey("Section3"), Is.True);
    }
    
    [Test]
    public async Task ReadAsync_SectionWithWhitespace_TrimsWhitespace()
    {
        // Arrange
        var content = "[  Section1  ]";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result.Sections.ContainsKey("Section1"), Is.True);
        Assert.That(result.Sections["Section1"].Name, Is.EqualTo("Section1"));
    }
    
    [Test]
    public async Task ReadAsync_KeyValueInRootSection_ParsesCorrectly()
    {
        // Arrange
        var content = "key1=value1";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues.ContainsKey("key1"), Is.True);
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value1"));
    }
    
    [Test]
    public async Task ReadAsync_KeyValueInSection_ParsesCorrectly()
    {
        // Arrange
        var content = @"[Section1]
key1=value1
key2=value2";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var section = result.Sections["Section1"];
        Assert.That(section.KeyValues.ContainsKey("key1"), Is.True);
        Assert.That(section.KeyValues["key1"].First(), Is.EqualTo("value1"));
        Assert.That(section.KeyValues.ContainsKey("key2"), Is.True);
        Assert.That(section.KeyValues["key2"].First(), Is.EqualTo("value2"));
    }
    
    [Test]
    public async Task ReadAsync_KeyValueWithSpaces_TrimsSpaces()
    {
        // Arrange
        var content = "  key1  =  value1  ";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value1"));
    }
    
    [Test]
    public async Task ReadAsync_KeyWithoutValue_DoesNotStoreKey()
    {
        // Arrange
        var content = "key1=";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        // Due to RemoveEmptyEntries, keys with empty values are not stored
        Assert.That(rootSection.KeyValues.ContainsKey("key1"), Is.False);
    }
    
    [Test]
    public async Task ReadAsync_KeyWithoutValue_KeepEmptyValuesOption_StoresKeyWithEmptyValue()
    {
        // Arrange
        var content = "key1=";
        var reader = new StringReader(content);
        var options = new IniOptions { KeepEmptyValues = true };
        var iniReader = new IniReader(options);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues.ContainsKey("key1"), Is.True);
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo(string.Empty));
    }
    
    [Test]
    public async Task ReadAsync_KeyWithEqualsInValue_ParsesCorrectly()
    {
        // Arrange
        var content = "key1=value=with=equals";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value=with=equals"));
    }
    
    [Test]
    public async Task ReadAsync_DuplicateKey_AppendsValue()
    {
        // Arrange
        var content = @"key1=value1
key1=value2
key1=value3";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues["key1"].Count(), Is.EqualTo(3));
        Assert.That(rootSection.KeyValues["key1"], Does.Contain("value1"));
        Assert.That(rootSection.KeyValues["key1"], Does.Contain("value2"));
        Assert.That(rootSection.KeyValues["key1"], Does.Contain("value3"));
    }
    
    [Test]
    public async Task ReadAsync_CommentLine_IgnoresComment()
    {
        // Arrange
        var content = @"; This is a comment
key1=value1";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues.Count, Is.EqualTo(1));
        Assert.That(rootSection.KeyValues.ContainsKey("key1"), Is.True);
    }
    
    [Test]
    public async Task ReadAsync_InlineComment_IsTreatedAsPartOfTheValue()
    {
        // Arrange
        var content = "key1=value1 ; This is a comment";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value1 ; This is a comment"));
    }
    
    [Test]
    public async Task ReadAsync_CustomCommentCharacter_UsesCustomCharacter()
    {
        // Arrange
        var content = @"# This is a comment
key1=value1";
        var reader = new StringReader(content);
        var options = new IniOptions { CommentCharacter = '#' };
        var iniReader = new IniReader(options);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues.Count, Is.EqualTo(1));
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value1"));
    }
    
    [Test]
    public async Task ReadAsync_EmptyLines_IgnoresEmptyLines()
    {
        // Arrange
        var content = @"key1=value1

key2=value2

";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues.Count, Is.EqualTo(2));
    }
    
    [Test]
    public async Task ReadAsync_WhitespaceOnlyLines_IgnoresLines()
    {
        // Arrange
        var content = @"key1=value1
   
	
key2=value2";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues.Count, Is.EqualTo(2));
    }
    
    [Test]
    public async Task ReadAsync_ComplexIniFile_ParsesCorrectly()
    {
        // Arrange
        var content = @"; This is a comment
globalKey=globalValue

[Database]
host=localhost
port=5432
; database config
username=admin
password=secret

[Application]
name=MyApp
version=1.0.0
debug=true

[Logging]
level=info
file=/var/log/app.log";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result.Sections.Count, Is.EqualTo(4)); // root + 3 sections
        
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues["globalKey"].First(), Is.EqualTo("globalValue"));
        
        var dbSection = result.Sections["Database"];
        Assert.That(dbSection.KeyValues["host"].First(), Is.EqualTo("localhost"));
        Assert.That(dbSection.KeyValues["port"].First(), Is.EqualTo("5432"));
        Assert.That(dbSection.KeyValues["username"].First(), Is.EqualTo("admin"));
        Assert.That(dbSection.KeyValues["password"].First(), Is.EqualTo("secret"));
        
        var appSection = result.Sections["Application"];
        Assert.That(appSection.KeyValues["name"].First(), Is.EqualTo("MyApp"));
        Assert.That(appSection.KeyValues["version"].First(), Is.EqualTo("1.0.0"));
        Assert.That(appSection.KeyValues["debug"].First(), Is.EqualTo("true"));
        
        var logSection = result.Sections["Logging"];
        Assert.That(logSection.KeyValues["level"].First(), Is.EqualTo("info"));
        Assert.That(logSection.KeyValues["file"].First(), Is.EqualTo("/var/log/app.log"));
    }
    
    [Test]
    public async Task ReadAsync_DuplicateSection_ReusesExistingSection()
    {
        // Arrange
        var content = @"[Section1]
key1=value1

[Section1]
key2=value2";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result.Sections.Count, Is.EqualTo(2)); // root + Section1 (not duplicated)
        var section = result.Sections["Section1"];
        Assert.That(section.KeyValues.Count, Is.EqualTo(2));
        Assert.That(section.KeyValues.ContainsKey("key1"), Is.True);
        Assert.That(section.KeyValues.ContainsKey("key2"), Is.True);
    }
    
    [Test]
    public async Task ReadAsync_InvalidLine_ThrowsFormatException()
    {
        // Arrange
        var content = @"key1=value1
invalid line without equals
key2=value2";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act & Assert
        var ex = Assert.ThrowsAsync<FormatException>(async () => await iniReader.ReadAsync(reader));
        Assert.That(ex.Message, Does.Contain("Syntax error at line 2"));
        Assert.That(ex.Message, Does.Contain("invalid line without equals"));
    }
    
    [Test]
    public async Task ReadAsync_WithCustomRootSection_UsesProvidedRootSection()
    {
        // Arrange
        var content = "key1=value1";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        var customRoot = new IniSection { Name = "CustomRoot" };
        
        // Act
        var result = await iniReader.ReadAsync(reader, customRoot);
        
        // Assert
        Assert.That(result.Sections[""], Is.SameAs(customRoot));
        Assert.That(customRoot.KeyValues.ContainsKey("key1"), Is.True);
    }
    
    [Test]
    public async Task ReadAsync_KeyWithOnlySpacesAfterEquals_DoesNotStoreKey()
    {
        // Arrange
        var content = "key1=   ";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        // Due to Trim() and RemoveEmptyEntries, keys with only whitespace values are not stored
        Assert.That(rootSection.KeyValues.ContainsKey("key1"), Is.False);
    }
    
    [Test]
    public async Task ReadAsync_SectionWithSpecialCharacters_ParsesCorrectly()
    {
        // Arrange
        var content = "[Section-1_Test.Name]";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result.Sections.ContainsKey("Section-1_Test.Name"), Is.True);
    }
    
    [Test]
    public async Task ReadAsync_ValueWithSpecialCharacters_PreservesValue()
    {
        // Arrange
        var content = "key1=value!@#$%^&*()_+-{}[]|:<>?,./~`";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value!@#$%^&*()_+-{}[]|:<>?,./~`"));
    }
    
    [Test]
    public async Task ReadAsync_KeysInDifferentSections_AreIndependent()
    {
        // Arrange
        var content = @"[Section1]
key1=value1

[Section2]
key1=value2";
        var reader = new StringReader(content);
        var iniReader = new IniReader(_defaultOptions);
        
        // Act
        var result = await iniReader.ReadAsync(reader);
        
        // Assert
        Assert.That(result.Sections["Section1"].KeyValues["key1"].First(), Is.EqualTo("value1"));
        Assert.That(result.Sections["Section2"].KeyValues["key1"].First(), Is.EqualTo("value2"));
    }

    [Test]
    public async Task ReadAsync_FilePath_ParsesCorrectly()
    {
        // Arrange
        var content = "key1=value1";
        var tempPath = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8);
            var iniReader = new IniReader(_defaultOptions);

            // Act
            var result = await iniReader.ReadAsync(tempPath);

            // Assert
            var rootSection = result.Sections[""];
            Assert.That(rootSection.KeyValues.ContainsKey("key1"), Is.True);
            Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value1"));
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Test]
    public async Task ReadAsync_Stream_ParsesCorrectly()
    {
        // Arrange
        var content = "key1=value1";
        var bytes = Encoding.UTF8.GetBytes(content);
        using var ms = new MemoryStream(bytes);
        var iniReader = new IniReader(_defaultOptions);

        // Act
        var result = await iniReader.ReadAsync(ms);

        // Assert
        var rootSection = result.Sections[""];
        Assert.That(rootSection.KeyValues.ContainsKey("key1"), Is.True);
        Assert.That(rootSection.KeyValues["key1"].First(), Is.EqualTo("value1"));
    }
}
