namespace Pypn.Core
{
    public interface ICommandParams<out TPayload> {
        TPayload Payload { get; }
        string CommandName { get; }
    }

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