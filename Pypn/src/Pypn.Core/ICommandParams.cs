namespace Pypn.Core
{
    public interface ICommandParams<out TPayload> {
        TPayload Payload { get; }
        string CommandName { get; }
    }
}