﻿
namespace Pypn.Core.Configuration
{
    public interface IStageCommandFactory<in TStage>
    {
        IStageCommand CreateForStage(TStage stage);

        ICommandDefinition<TStage> StageCommandDefinition { get; }
    }

    public interface IStageCommandFactory<in TStage, in TPayload> : IStageCommandFactory<TStage>
    {
        ICommandDefinition<TStage, TPayload> CommandDefinition { get; }
    }

    /// <summary>
    /// Used internally by the framework to create instances of IStageCommand.
    /// </summary>
    /// <typeparam name="TStage"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    public sealed class StageCommandFactory<TStage, TPayload> : IStageCommandFactory<TStage, TPayload>
    {
        public StageCommandFactory(ICommandDefinition<TStage, TPayload> commandDefinition)
        {
            CommandDefinition = commandDefinition;
        }

        public ICommandDefinition<TStage, TPayload> CommandDefinition { get; }

        public ICommandDefinition<TStage> StageCommandDefinition { get { return CommandDefinition; } }

        public IStageCommand CreateForStage(TStage stage)
        {
            return new StageCommand<TStage, TPayload>(stage, CommandDefinition);
        }
    }
}
