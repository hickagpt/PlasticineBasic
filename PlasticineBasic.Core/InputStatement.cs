namespace PlasticineBasic.Core
{
    public class InputStatement : StatementNode
    {
        #region Public Properties

        public List<VariableReference> Variables { get; set; } = new();

        #endregion Public Properties
    }
}