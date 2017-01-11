using System;

namespace Pypn.Core.Configuration
{

    /// <summary>
    /// A default implementation of ICommandDefinition however, if needed, clients of this framework can provide custom implementations of this to 
    /// avoid the overhead of calling delegates
    /// </summary>
    /// <typeparam name="TStage"></typeparam>
    /// <typeparam name="Payload"></typeparam>
    public class CommandDefinition<TStage, TPayload> : ICommandDefinition<TStage, TPayload>
    {
        private readonly Func<TStage, ICommandParams<TPayload>, PipelineAction> _command;
        private readonly Func<TStage, ICommandParams<TPayload>, PipelineAction> _postCommand;

        public CommandDefinition(Action<TStage, ICommandParams<TPayload>> command,
            Action<TStage, ICommandParams<TPayload>> postCommand = null)
        {
            if(command != null)
            {
                _command = (s, arg) => { command(s, arg); return PipelineAction.Continue; };
            }
            if (postCommand != null)
            {
                _postCommand = (s, arg) => { postCommand(s, arg); return PipelineAction.Continue; };
            }
        }

        public CommandDefinition(Func<TStage, ICommandParams<TPayload>, PipelineAction> command,
            Func<TStage, ICommandParams<TPayload>, PipelineAction> postCommand = null)
        {
            _command = command;
            _postCommand = postCommand;
        }

        public PipelineAction RunCommand(TStage stage, ICommandParams<TPayload> callContext)
        {
            if (_command != null)
            {
                return _command(stage, callContext);
            }
            return PipelineAction.Continue;
        }

        public PipelineAction RunPostCommand(TStage stage, ICommandParams<TPayload> callContext)
        {
            if (_postCommand != null)
            {
                return _postCommand(stage, callContext);
            }
            return PipelineAction.Continue;
        }
    }
}