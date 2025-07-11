namespace PlasticineBasic.Core
{
    public class ExecutionContext
    {
        #region Public Properties

        public bool IsRunning { get; set; } = true;
        public Dictionary<string, object> Variables { get; } = new();

        #endregion Public Properties
    }
}