using Pypn.Core.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Pypn.Core.Configuration
{
    
    /// <summary>
    /// Note: This implementation should only be considered thread-safe once ALL commands have been added; This should be sufficient for most applications, but an alternative
    /// implementation can easily be provided as part of StageConfig{TStage} constructor
    /// </summary>
    /// <typeparam name="TStage"></typeparam>
    public class StageCommandConfig<TStage> : IStageCommandConfig<TStage>
    {
        private readonly IDictionary<string, List<IStageCommandFactory<TStage>>> _commands = new Dictionary<string, List<IStageCommandFactory<TStage>>>();
        private ISessionCommandDefinition<TStage> _startSessionCommand = null;
        private ISessionCommandDefinition<TStage> _abortSessionCommand = null;
        private ISessionCommandDefinition<TStage> _endSessionCommand = null;

        public void DefineCommand<TPayload>(string commandName, ICommandDefinition<TStage, TPayload> command)
        {
            var key = commandName ?? string.Empty;
            List<IStageCommandFactory<TStage>> commands;
            if(!_commands.TryGetValue(key, out commands))
            {
                commands = new List<IStageCommandFactory<TStage>>();
                _commands[key] = commands;
            }

            commands.Add(new StageCommandFactory<TStage, TPayload>(command));
        }

        public void DefineStartSessionCommand(ISessionCommandDefinition<TStage> command)
        {
            _startSessionCommand = command;
        }

        public void DefineAbortSessionCommand(ISessionCommandDefinition<TStage> command)
        {
            _abortSessionCommand = command;
        }

        public void DefineEndSessionCommand(ISessionCommandDefinition<TStage> command)
        {
            _endSessionCommand = command;
        }

        public IEnumerable<IStageCommand> GetStageCommands(string commandName, TStage stage)
        {
            List<IStageCommandFactory<TStage>> commands;
            var key = commandName ?? string.Empty;
            if (_commands.TryGetValue(key, out commands))
            {
                return commands.Select(c => c.CreateForStage(stage));
            }
            return new IStageCommand[0];
        }

        public IStageCommand<TPayload> GetStageCommand<TPayload>(string commandName, TStage stage)
        {
            var key = commandName ?? string.Empty;
            List<IStageCommandFactory<TStage>> commands;
            if (_commands.TryGetValue(key, out commands))
            {
                var factory = commands.OfType<IStageCommandFactory<TStage, TPayload>>().FirstOrDefault();
                if(factory != null)
                {
                    return (IStageCommand<TPayload>)factory.CreateForStage(stage);
                }
            }
            return null;
        }

        public ISessionCommandDefinition<TStage> GetStartSessionCommand()
        {
            return _startSessionCommand;
        }

        public ISessionCommandDefinition<TStage> GetAbortSessionCommand()
        {
            return _abortSessionCommand;
        }

        public ISessionCommandDefinition<TStage> GetEndSessionCommand()
        {
            return _endSessionCommand;
        }
    }
}