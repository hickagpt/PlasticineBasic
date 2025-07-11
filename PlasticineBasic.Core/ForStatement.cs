using PlasticineBasic.Core;

public class ForStatement : StatementNode
{
    #region Public Properties

    public ExpressionNode End { get; set; }
    public ExpressionNode Start { get; set; }
    public ExpressionNode Step { get; set; }
    public string VariableName { get; set; }

    #endregion Public Properties

    // Optional, can be null (default to 1)
}
