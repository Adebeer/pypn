namespace Pypn.Core
{
    public interface IStageCommand { }

    public interface IStageCommand<in TPayload> : IStageCommand
    {
        PipelineAction RunCommand(ICommandParams<TPayload> callContext);
        PipelineAction RunPostCommand(ICommandParams<TPayload> callContext);
    }

    public interface IStageSessionCommand : IStageCommand
    {
        PipelineAction RunCommand();
        PipelineAction RunPostCommand();
    }
}