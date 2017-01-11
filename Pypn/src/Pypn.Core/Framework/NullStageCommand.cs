namespace Pypn.Core.Framework
{

    public sealed class NullStageCommand<TPayload> : IStageCommand<TPayload>
    {
        public PipelineAction RunCommand(ICommandParams<TPayload> callContext)
        {
            return PipelineAction.Continue;
        }

        public PipelineAction RunPostCommand(ICommandParams<TPayload> callContext)
        {
            return PipelineAction.Continue;
        }
    }
}