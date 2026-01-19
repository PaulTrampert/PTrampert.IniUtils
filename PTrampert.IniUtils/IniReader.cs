using System.IO;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace PTrampert.IniUtils;

public class IniReader
{
    public IniFile Read(TextReader reader)
    {
        var antlrStream = new AntlrInputStream(reader);
        var lexer = new IniLexer(antlrStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new IniGrammar(tokenStream);
        var tree = parser.iniFile();
        var visitor = new IniTreeVisitor();
        return visitor.Visit(tree);
    }
}



