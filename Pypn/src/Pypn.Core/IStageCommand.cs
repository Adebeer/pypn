using System;
using Pypn.Core.Configuration;

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