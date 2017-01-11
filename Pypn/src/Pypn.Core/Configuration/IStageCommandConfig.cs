using System.Collections.Generic;

namespace Pypn.Core.Configuration
{
    public interface IStageCommandConfig<TStage> 
    {
        /// <summary>
        /// Defines a command that can be executed against a particular TStage implementation
        /// NB: The Order in which commands are defined is VERY important - when running a command, the pipeline engine will locate and run the FIRST
        /// defined command with TPayload that is compatible (i.e. caller command TPayload derives from defined command TPayload) with the supplied TPayload. 
        /// This is really the simplest/best strategy as things can get VERY complex/confusing given that TPayload could implement multiple interfaces and you could define commands
        /// on any one or combinations of these interfaces...which defined command should be called?? Simple...the first one that matches!
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="commandName"></param>
        /// <param name="command"></param>
        void DefineCommand<TPayload>(string commandName, ICommandDefinition<TStage, TPayload> command);

        void DefineStartSessionCommand(ISessionCommandDefinition<TStage> command);
        void DefineAbortSessionCommand(ISessionCommandDefinition<TStage> command);
        void DefineEndSessionCommand(ISessionCommandDefinition<TStage> command);

        IStageCommand<TPayload> GetStageCommand<TPayload>(string commandName, TStage stage);
        IEnumerable<IStageCommand> GetStageCommands(string commandName, TStage stage);

        ISessionCommandDefinition<TStage> GetStartSessionCommand();
        ISessionCommandDefinition<TStage> GetAbortSessionCommand();
        ISessionCommandDefinition<TStage> GetEndSessionCommand();
    }
}