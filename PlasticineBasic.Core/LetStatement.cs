namespace PlasticineBasic.Core
{
    public class LetStatement : StatementNode
    {
        #region Public Properties

        public ExpressionNode Expression { get; set; }
        public string VariableName { get; set; }

        #endregion Public Properties
    }
}