using PlasticineBasic.Core;

namespace PlasticBasic.Core.Tests;

public class TokeniserTests
{
    #region Public Methods

    [Fact]
    public void TokeniseInputLine()
    {
        var source = "INPUT x";
        var tokeniser = new Tokeniser(source);
        var tokens = tokeniser.Tokenise();
        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Input, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[2].Type);
    }

    [Fact]
    public void TokeniseLetLine()
    {
        var source = "LET x = 10";

        var tokeniser = new Tokeniser(source);

        var tokens = tokeniser.Tokenise();

        Assert.Equal(5, tokens.Count);
        Assert.Equal(TokenType.Let, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.Assign, tokens[2].Type);
        Assert.Equal(TokenType.NumberLiteral, tokens[3].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[4].Type);
    }

    [Fact]
    public void TokenisePrintLiteralLine()
    {
        var source = "PRINT \"Hello\"";

        var tokeniser = new Tokeniser(source);

        var tokens = tokeniser.Tokenise();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Print, tokens[0].Type);
        Assert.Equal(TokenType.StringLiteral, tokens[1].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[2].Type);
    }

    [Fact]
    public void TokenisePrintVariableLine()
    {
        var source = "PRINT x";

        var tokeniser = new Tokeniser(source);

        var tokens = tokeniser.Tokenise();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Print, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[2].Type);
    }

    #endregion Public Methods
}