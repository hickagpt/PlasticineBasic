namespace PlasticineBasic.Core
{
    public class Token
    {
        #region Public Constructors

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        #endregion Public Constructors

        #region Public Properties

        public int Column { get; }
        public int Line { get; }
        public TokenType Type { get; }
        public string Value { get; }

        #endregion Public Properties

        #region Public Methods

        public override string ToString() => $"{Type} '{Value}' @ {Line}:{Column}";

        #endregion Public Methods
    }
}