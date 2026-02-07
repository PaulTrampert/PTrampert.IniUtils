namespace PTrampert.IniUtils.Test;

public class IniSyntaxExceptionTests
{
    [Test]
    public void IniSyntaxException_MessageIncludesLineNumberAndContent_WithoutFilePath()
    {
        // Arrange
        const int lineNumber = 42;
        const string lineContent = "invalid_line";

        // Act
        var ex = new IniSyntaxException(lineNumber, lineContent, null);

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Syntax error at line 42: 'invalid_line'"));
    }
    
    [Test]
    public void IniSyntaxException_MessageIncludesLineNumberContentAndFilePath()
    {
        // Arrange
        const int lineNumber = 7;
        const string lineContent = "another_invalid_line";
        const string filePath = "config.ini";
        
        // Act
        var ex = new IniSyntaxException(lineNumber, lineContent, filePath);
        
        // Assert
        Assert.That(ex.Message, Is.EqualTo("Syntax error in 'config.ini' at line 7: 'another_invalid_line'"));
    }

    [Test]
    public void IniSyntaxException_PropertiesAreSetCorrectly()
    {
        const int lineNumber = 10;
        const string lineContent = "bad_line";
        const string filePath = "settings.ini";
        var ex = new IniSyntaxException(lineNumber, lineContent, filePath);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex.LineNumber, Is.EqualTo(lineNumber));
            Assert.That(ex.LineContent, Is.EqualTo(lineContent));
            Assert.That(ex.FilePath, Is.EqualTo(filePath));
        }
    }
}