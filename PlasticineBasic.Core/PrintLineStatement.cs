namespace PlasticineBasic.Core;

public class PrintLineStatement : StatementNode
{
    #region Public Properties

    public List<ExpressionNode> Values { get; set; } = new();

    #endregion Public Properties
}