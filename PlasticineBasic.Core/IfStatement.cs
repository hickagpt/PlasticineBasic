namespace PlasticineBasic.Core
{
    public class IfStatement : StatementNode
    {
        #region Public Properties

        public ExpressionNode Condition { get; set; }
        public StatementNode ThenBranch { get; set; }

        #endregion Public Properties
    }
}