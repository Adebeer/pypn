namespace Pypn.Core.Framework
{
    /// <summary>
    /// Used internally by the framework
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public sealed class CommandParams<TPayload> : ICommandParams<TPayload>
    {
        public CommandParams(string commandName)
        {
            CommandName = commandName;
        }

        public TPayload Payload { get; set; }
        public string CommandName { get; }
    }
}