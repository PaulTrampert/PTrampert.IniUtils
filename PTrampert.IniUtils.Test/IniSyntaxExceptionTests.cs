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

    [Test]
    public void IniSyntaxException_ParameterlessConstructor_CreatesExceptionWithDefaultValues()
    {
        // Act
        var ex = new IniSyntaxException();
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex.LineNumber, Is.EqualTo(0));
            Assert.That(ex.LineContent, Is.EqualTo(string.Empty));
            Assert.That(ex.FilePath, Is.Null);
            Assert.That(ex.Message, Is.Not.Null);
        }
    }

    [Test]
    public void IniSyntaxException_MessageConstructor_SetsCustomMessage()
    {
        // Arrange
        const string customMessage = "Custom error message";
        
        // Act
        var ex = new IniSyntaxException(customMessage);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex.Message, Is.EqualTo(customMessage));
            Assert.That(ex.LineNumber, Is.EqualTo(0));
            Assert.That(ex.LineContent, Is.EqualTo(string.Empty));
            Assert.That(ex.FilePath, Is.Null);
        }
    }

    [Test]
    public void IniSyntaxException_MessageAndInnerExceptionConstructor_SetsMessageAndInnerException()
    {
        // Arrange
        const string customMessage = "Custom error with inner exception";
        var innerException = new InvalidOperationException("Inner exception message");
        
        // Act
        var ex = new IniSyntaxException(customMessage, innerException);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex.Message, Is.EqualTo(customMessage));
            Assert.That(ex.InnerException, Is.SameAs(innerException));
            Assert.That(ex.LineNumber, Is.EqualTo(0));
            Assert.That(ex.LineContent, Is.EqualTo(string.Empty));
            Assert.That(ex.FilePath, Is.Null);
        }
    }

    [Test]
    public void IniSyntaxException_PropertiesCanBeSetAfterConstruction()
    {
        // Arrange
        var ex = new IniSyntaxException("Test message");
        const int lineNumber = 15;
        const string lineContent = "test_content";
        const string filePath = "test.ini";
        
        // Act
        ex.LineNumber = lineNumber;
        ex.LineContent = lineContent;
        ex.FilePath = filePath;
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex.LineNumber, Is.EqualTo(lineNumber));
            Assert.That(ex.LineContent, Is.EqualTo(lineContent));
            Assert.That(ex.FilePath, Is.EqualTo(filePath));
        }
    }
}