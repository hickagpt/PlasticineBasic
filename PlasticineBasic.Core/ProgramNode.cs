namespace PlasticineBasic.Core
{
    public class ProgramNode : AstNode
    {
        #region Public Properties

        public List<StatementNode> Statements { get; } = new();

        #endregion Public Properties
    }
}