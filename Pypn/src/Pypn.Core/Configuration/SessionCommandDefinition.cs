using System;

namespace Pypn.Core.Configuration
{

    /// <summary>
    /// A default implementation of ISessionCommandDefinition however, if needed, clients of this framework can provide custom implementations of this to avoid overhead 
    /// of calling delegates
    /// </summary>
    public class SessionCommandDefinition<TStage> : ISessionCommandDefinition<TStage>
    {
        private readonly Func<TStage, PipelineAction> _sessionCommand;
        private readonly Func<TStage, PipelineAction> _postSessionCommand;

        public SessionCommandDefinition(Func<TStage, PipelineAction> sessionCommand, Func<TStage, PipelineAction> postSessionCommand = null)
        {
            _sessionCommand = sessionCommand;
            _postSessionCommand = postSessionCommand;
        }

        public SessionCommandDefinition(Action<TStage> sessionCommand, Action<TStage> postSessionCommand = null)
        {
            if(sessionCommand != null)
            {
                _sessionCommand = s => { sessionCommand(s); return PipelineAction.Continue; };
            }

            if (postSessionCommand != null)
            {
                _postSessionCommand = s => { postSessionCommand(s); return PipelineAction.Continue; };
            }
        }

        public PipelineAction RunCommand(TStage stage)
        {
            if(_sessionCommand != null)
            {
                return _sessionCommand(stage);
            }
            return PipelineAction.Continue;
        }

        public PipelineAction RunPostCommand(TStage stage)
        {
            if (_postSessionCommand != null)
            {
                return _postSessionCommand(stage);
            }
            return PipelineAction.Continue;
        }
    }
}