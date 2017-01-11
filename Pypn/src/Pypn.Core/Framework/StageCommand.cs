using Pypn.Core.Configuration;

namespace Pypn.Core.Framework
{
    public sealed class StageCommand<TStage, TPayload> : IStageCommand<TPayload>
    {
        private readonly TStage _stage;
        private readonly ICommandDefinition<TStage, TPayload> _commandDefinition;

        public StageCommand(TStage stage, ICommandDefinition<TStage, TPayload> commandDefinition)
        {
            _stage = stage;
            _commandDefinition = commandDefinition;
        }

        public PipelineAction RunCommand(ICommandParams<TPayload> callContext)
        {
            return _commandDefinition.RunCommand(_stage, callContext);
        }

        public PipelineAction RunPostCommand(ICommandParams<TPayload> callContext)
        {
            return _commandDefinition.RunPostCommand(_stage, callContext);
        }
    }
}