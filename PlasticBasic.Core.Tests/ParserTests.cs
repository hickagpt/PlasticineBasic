using PlasticineBasic.Core;

namespace PlasticBasic.Core.Tests;

public class ParserTests
{
    #region Public Methods

    [Fact]
    public void ParseLetStatement()
    {
        var source = "10 LET x = 1 + 1";
        var tokeniser = new Tokeniser(source);
        var tokens = tokeniser.Tokenise();
        var parser = new Parser(tokens);
        var program = parser.Parse();

        Assert.Single(program.Statements);
    }

    #endregion Public Methods
}