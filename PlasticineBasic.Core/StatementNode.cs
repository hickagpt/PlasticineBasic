namespace PlasticineBasic.Core
{
    public abstract class StatementNode : AstNode
    {
        #region Public Properties

        public int? LineNumber { get; set; }

        #endregion Public Properties
    }
}