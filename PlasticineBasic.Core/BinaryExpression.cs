namespace PlasticineBasic.Core
{
    public class BinaryExpression : ExpressionNode
    {
        #region Public Properties

        public ExpressionNode Left { get; set; }
        public string Operator { get; set; } // "+", "-", "*", etc.
        public ExpressionNode Right { get; set; }

        #endregion Public Properties
    }
}