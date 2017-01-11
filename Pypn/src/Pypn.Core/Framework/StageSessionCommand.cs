using Pypn.Core.Configuration;

namespace Pypn.Core.Framework
{
    public sealed class StageSessionCommand<TStage> : IStageSessionCommand
    {
        private readonly TStage _stage;
        private readonly ISessionCommandDefinition<TStage> _commandDefinition;

        public StageSessionCommand(TStage stage, ISessionCommandDefinition<TStage> commandDefinition)
        {
            _stage = stage;
            _commandDefinition = commandDefinition;
        }

        public PipelineAction RunCommand()
        {
            return _commandDefinition.RunCommand(_stage);
        }

        public PipelineAction RunPostCommand()
        {
            return _commandDefinition.RunPostCommand(_stage);
        }
    }
}