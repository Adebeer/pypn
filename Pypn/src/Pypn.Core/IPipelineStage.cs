using Pypn.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pypn.Core
{
    public enum SessionStatus
    {
        NeverStarted = 0,
        Starting = 1,
        Started = 2,
        Aborting = 3,
        Aborted = 4,
        Ending = 5,
        Ended = 6,
    }

    /// <summary>
    /// Pipeline instantiation of IStageAdaptor so can be IDisposable and tied to Pipeline instance
    /// </summary>
    public interface IPipelineStage : IDisposable {

        IStageConfig Config { get; }

        /// <summary>
        /// This method should call StageConfig.Commands to create/cache commands 
        /// Or simply Run... 
        /// </summary>
        IStageCommand<TPayload> RunCommand<TPayload>(string commandName, bool canCacheCommand = true);
        void StartSession();
        void AbortSession();
        void EndSession();
        void PostStartSession();
        void PostAbortSession();
        void PostEndSession();

        SessionStatus SessionStatus { get; }
        
        bool SessionStarted { get; }
        bool SessionAborted { get; }

        object GetStageInstance();
    }

    public sealed class PipelineStage<TStage> : IPipelineStage 
        where TStage : class
    {
        private readonly IStageConfig<TStage> _config;
        private readonly Lazy<IStageSessionCommand> _startCommand;
        private readonly Lazy<IStageSessionCommand> _abortCommand;
        private readonly Lazy<IStageSessionCommand> _endCommand;
        private readonly IDictionary<string, List<IStageCommand>> _commandCache;

        public PipelineStage(IStageConfig<TStage> stageConfig, TStage stage)
        {
            Stage = stage;
            _config = stageConfig;

            _startCommand = new Lazy<IStageSessionCommand>(() => CreateStageSessionCommand(_config.CommandConfig.GetStartSessionCommand()), false);
            _abortCommand = new Lazy<IStageSessionCommand>(() => CreateStageSessionCommand(_config.CommandConfig.GetAbortSessionCommand()), false);
            _endCommand = new Lazy<IStageSessionCommand>(() => CreateStageSessionCommand(_config.CommandConfig.GetEndSessionCommand()), false);
            _commandCache = new Dictionary<string, List<IStageCommand>>();
        }

        public TStage Stage { get; private set; }

        public IStageConfig Config { get { return _config; } }

        public bool SessionAborted { get { return SessionStatus == SessionStatus.Aborting || SessionStatus == SessionStatus.Aborted; } }

        public bool SessionStarted { get { return SessionStatus == SessionStatus.Starting || SessionStatus == SessionStatus.Started; } }

        public IStageCommand<TPayload> RunCommand<TPayload>(string commandName, bool canCacheCommand = true)
        {
            IStageCommand<TPayload> stageCommand = null;
            if (canCacheCommand) 
            {
                List<IStageCommand> cachedCommands;
                var key = commandName ?? string.Empty;
                if (_commandCache.TryGetValue(key, out cachedCommands) == false)
                {
                    // we need to create all the stage commands upfront otherwise, due to polymorphic behaviour, if we create each stage command on a per-request basis, we
                    // may end up returning different implementations of IStageCommand<TPayload> if there are polymorphic relationships between different TPayload 
                    // ICommandDefinition implementations for the same commandName
                    _commandCache[key] = cachedCommands = _config.CommandConfig.GetStageCommands(commandName, Stage).ToList();
                }

                stageCommand = cachedCommands.OfType<IStageCommand<TPayload>>().FirstOrDefault();
            }
            else
            {
                stageCommand = _config.CommandConfig.GetStageCommand<TPayload>(commandName, Stage);
            }
            if (stageCommand != null)
            {
                return stageCommand;
            }
            if(string.IsNullOrEmpty(commandName) == false)
            {
                return RunCommand<TPayload>(null, canCacheCommand); // return default command, if any
            }
            return null;
        }

        public SessionStatus SessionStatus { get; private set; }

        public void StartSession()
        {
            switch(SessionStatus)
            {
                case SessionStatus.NeverStarted:
                case SessionStatus.Aborted:
                case SessionStatus.Ended:
                    if (_startCommand.Value != null)
                    {
                        _startCommand.Value.RunCommand();
                    }
                    SessionStatus = SessionStatus.Starting;
                    break;
            }
        }

        public void PostStartSession()
        {
            if (SessionStatus != SessionStatus.Starting) { return; }

            if (_startCommand.Value != null)
            {
                _startCommand.Value.RunPostCommand();
            }
            SessionStatus = SessionStatus.Started;
        }

        public void AbortSession()
        {
            if(SessionStatus != SessionStatus.Starting && SessionStatus != SessionStatus.Started) { return; }

            if (_abortCommand.Value != null)
            {
                _abortCommand.Value.RunCommand();
            }
            SessionStatus = SessionStatus.Aborting;
        }

        public void PostAbortSession()
        {
            if(SessionStatus != SessionStatus.Aborting) { return; }

            if (_abortCommand.Value != null)
            {
                _abortCommand.Value.RunPostCommand();
            }
            SessionStatus = SessionStatus.Aborted;
        }

        public void EndSession()
        {
            if(SessionStatus != SessionStatus.Started) { return; }
            if (_endCommand.Value != null)
            {
                _endCommand.Value.RunCommand();
            }
            SessionStatus = SessionStatus.Ending;
        }

        public void PostEndSession()
        {
            if(SessionStatus != SessionStatus.Ending) { return; }
            if (_endCommand.Value != null)
            {
                _endCommand.Value.RunPostCommand();
            }
            SessionStatus = SessionStatus.Ended;
        }

        private IStageSessionCommand CreateStageSessionCommand(ISessionCommandDefinition<TStage> cmdDefinition)
        {
            return cmdDefinition != null
                ? new StageSessionCommand<TStage>(Stage, cmdDefinition)
                : null;
        }

        public object GetStageInstance()
        {
            return Stage;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // Normally the Pipeline should call Abort/PostAbortSession (or End/PostEndSession). But in case it didn't, this 
                // is our last resort in attempting cleanup
                AbortSession();
                PostAbortSession(); 
                if (disposing)
                {
                    (Stage as IDisposable)?.Dispose();
                }

                _commandCache.Clear();
                Stage = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}