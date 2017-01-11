using System;

namespace Pypn.Core.Configuration
{
    /// <summary>
    /// Extension methods to provide a basic Fluent interface for defining TStage commands
    /// </summary>
    public static class FluentStageCommands
    {
        public static IStageCommandConfig<TStage> WithCommand<TStage, TPayload>(this IStageCommandConfig<TStage> stageConfig, string commandName, 
            Action<TStage, ICommandParams<TPayload>> command,
            Action<TStage, ICommandParams<TPayload>> postCommand = null)
        {
            stageConfig.DefineCommand(commandName, new CommandDefinition<TStage, TPayload>(command, postCommand));
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithCommand<TStage, TPayload>(this IStageCommandConfig<TStage> stageConfig, string commandName,
            Func<TStage, ICommandParams<TPayload>, PipelineAction> command,
            Func<TStage, ICommandParams<TPayload>, PipelineAction> postCommand = null)
        {
            stageConfig.DefineCommand(commandName, new CommandDefinition<TStage, TPayload>(command, postCommand));
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithCommand<TStage, TPayload>(this IStageCommandConfig<TStage> stageConfig, string commandName,
            ICommandDefinition<TStage, TPayload> commandDefinition)
        {
            stageConfig.DefineCommand(commandName, commandDefinition);
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithStartSessionCommand<TStage>(this IStageCommandConfig<TStage> stageConfig, Action<TStage> sessionCommand, Action<TStage> postSessionCommand = null)
        {
            stageConfig.DefineStartSessionCommand(new SessionCommandDefinition<TStage>(sessionCommand, postSessionCommand));
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithAbortSessionCommand<TStage>(this IStageCommandConfig<TStage> stageConfig, Action<TStage> sessionCommand, Action<TStage> postSessionCommand = null)
        {
            stageConfig.DefineAbortSessionCommand(new SessionCommandDefinition<TStage>(sessionCommand, postSessionCommand));
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithEndSessionCommand<TStage>(this IStageCommandConfig<TStage> stageConfig, Action<TStage> sessionCommand, Action<TStage> postSessionCommand = null)
        {
            stageConfig.DefineEndSessionCommand(new SessionCommandDefinition<TStage>(sessionCommand, postSessionCommand));
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithStartSessionCommand<TStage>(this IStageCommandConfig<TStage> stageConfig, ISessionCommandDefinition<TStage> sessionCommand)
        {
            stageConfig.DefineStartSessionCommand(sessionCommand);
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithAbortSessionCommand<TStage>(this IStageCommandConfig<TStage> stageConfig, ISessionCommandDefinition<TStage> sessionCommand)
        {
            stageConfig.DefineAbortSessionCommand(sessionCommand);
            return stageConfig;
        }

        public static IStageCommandConfig<TStage> WithEndSessionCommand<TStage>(this IStageCommandConfig<TStage> stageConfig, ISessionCommandDefinition<TStage> sessionCommand)
        {
            stageConfig.DefineEndSessionCommand(sessionCommand);
            return stageConfig;
        }
    }
}
