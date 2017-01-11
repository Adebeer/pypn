using Pypn.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pypn.Core
{
    /// <summary>
    /// IPipeline consists of a series of stages (TStage). Use an instance of IPipeline to run command(s) via one or mare stages. 
    /// 
    /// The stages and commands supported via a pipeline is configured via <see cref="IPipelineConfig",/>. A configured 
    /// IPipelineConfig acts as a factory for IPipeline instances.
    /// You can create as many IPipeline instances as you want, but each IPipeline instance is, by design, not thread safe
    /// and intended to be only used from a single thread.
    /// 
    /// Commands are really adaptors that allow you to present the same TPayload information to different TStage implementations.
    /// These TPayload types encapsulate all input/output parameters you need as part of your pipeline commands/operations.
    /// As such, when you use this pipeline framework to create your own custom pipeline-based implementations/frameworks, it is 
    /// recommended that you create a simple wrapper classes for your IPipelineConfig and IPipeline instances in order to present
    /// a more natural/simpler interface to your framework users.
    /// </summary>
    /// <example>
    /// As an example of how to apply this in a real world scenario, consider how you'd use this if you were building an ORM.
    /// Here a IPipelineConfig is analogous to a configured data model or session factory. It can't be used directly
    /// to run queries/statements on a database. Instead you use the configured data model/IPipelineConfig to create one or more
    /// sessions (IPipeline instances) - each session is tied to a particular thread. You can also use the session commands on a
    /// pipeline instance to start (create)/abort (rollback) or end (commit) transactions.
    /// </example>
    public interface IPipeline : IDisposable
    {
        IReadOnlyList<IPipelineStage> Stages { get; }

        /// <summary>
        /// NB: The Order in which commands are defined is VERY important - when running a command, the pipeline engine will locate 
        /// a compatible defined command by finding a match on the FIRST defined command that
        /// matches by commandName and, secondly, by type of TPayload
        /// Future implementations may support other modes for running commands - e.g. All vs FirstMatch...
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="commandName"></param>
        /// <param name="payload"></param>
        void RunCommand<TPayload>(string commandName, TPayload payload);

        //TODO: RunCommand(string commandName, object payload);
        //TODO: Other types of options for running TStage command(s). Right now we only support FirstMatch, but we could also support All

        /// <summary>
        /// Similar to RunCommand except that it will run the supplied command on TStage; all other TStage implementations will run
        /// the normal commands (as per given commandName and TPayload type)
        /// </summary>
        /// <typeparam name="TStage"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="commandName"></param>
        /// <param name="payload"></param>
        /// <param name="command"></param>
        void RunAdHocCommand<TStage, TPayload>(string commandName, TPayload payload, ICommandDefinition<TStage, TPayload> command)
            where TStage : class;

        /// <summary>
        /// Start a session. This will invoke the StartSession command on each pipeline stage.
        /// For example, a database pipeline stage would typically start a database session and potentially even a transaction
        /// </summary>
        void StartSession();

        /// <summary>
        /// This will invoke the AbortSession command on each pipeline stage. 
        /// This command is also called on Pipeline Dispose() IF and only IF StartSession() was called and the session is still active (i.e. not aborted) prior to dispose
        /// For example, a database pipeline stage would typically rollback a transaction here (assuming its StartSession started a transaction)
        /// </summary>
        void AbortSession();

        /// <summary>
        /// This will invoke the EndSession command on each pipeline stage.
        /// For example, a database pipeline stage would typically commit a transaction here (assuming its StartSession started a transaction)
        /// </summary>
        void EndSession();

        /// <summary>
        /// Returns true if a Session is active. Specifically, this means that StartSession() was called without a subsequent Abort/EndSession() call.
        /// This property will also return true if Abort/EndSession() was called, but an exception was thrown as part of this process hence leaving some of the Stages in a SessionStarted state.
        /// Note: Upon Disposal of pipeline, if SessionStarted == true, then AbortSesion() will be called.
        /// </summary>
        bool SessionStarted { get; }

        /// <summary>
        /// Returns true if session was most recently aborted. This value will reset to false after calling Start/EndSession(); 
        /// This property will also return true if AbortSession() has previously been called, but an exception was thrown as part of this process hence leaving some of the Stages in a SessionAborted state.
        /// Note: Upon Disposal of pipeline, if SessionStarted == true, then AbortSesion() will be called.
        /// </summary>
        bool SessionAborted { get; }
    }

    public sealed class Pipeline : IPipeline
    {
        private static readonly Action<IPipelineStage> startSessionCommand = s => s.StartSession();
        private static readonly Action<IPipelineStage> postStartSessionCommand = s => s.PostStartSession();
        private static readonly Action<IPipelineStage> abortSessionCommand = s => s.AbortSession();
        private static readonly Action<IPipelineStage> postAbortSessionCommand = s => s.PostAbortSession();
        private static readonly Action<IPipelineStage> endSessionCommand = s => s.EndSession();
        private static readonly Action<IPipelineStage> postEndSessionCommand = s => s.PostEndSession();

        private bool _sessionStarted;
        private bool _sessionAborted;

        private class StageCallContext<TPayload>
        {
            public StageCallContext(string commandName, ICommandParams<TPayload> commandParams, int maxStageIndex, IDictionary<string, IStageCommand<TPayload>> customStageCommands = null)
            {
                CommandName = commandName;
                CommandParams = commandParams;
                MaxStageIndex = maxStageIndex;
                MostRecentCommandAction = PipelineAction.Continue;
                CustomStageCommands = customStageCommands ?? new Dictionary<string, IStageCommand<TPayload>>();
            }

            public string CommandName { get; }
            public ICommandParams<TPayload> CommandParams { get; }
            public int MaxStageIndex { get; }
            public PipelineAction MostRecentCommandAction { get; set; }
            public IDictionary<string, IStageCommand<TPayload>> CustomStageCommands { get; }

            public static readonly IStageCommand<TPayload> NullStageCommand = new NullStageCommand<TPayload>();
        }

        private class SessionCallContext
        {
            public SessionCallContext(Action<IPipelineStage> sessionCommand, Action<IPipelineStage> postSessionCommand)
            {
                SessionCommand = sessionCommand;
                PostSessionCommand = postSessionCommand;
                Exceptions = new List<Exception>();
            }

            public Action<IPipelineStage> SessionCommand { get; }
            public Action<IPipelineStage> PostSessionCommand { get; }

            public List<Exception> Exceptions { get; }
        }

        public Pipeline(IEnumerable<IStageConfig> stages)
        {
            Stages = new ReadOnlyCollection<IPipelineStage>(stages.Select(x => x.CreateStage()).ToArray());
        }

        public IReadOnlyList<IPipelineStage> Stages { get; private set; }

        public void RunCommand<TPayload>(string commandName, TPayload payload)
        {
            var commandParams = new CommandParams<TPayload>(commandName)
            {
                Payload = payload
            };

            var callContext = new StageCallContext<TPayload>(commandName, commandParams, Stages.Count);
            if (callContext.MaxStageIndex > 0)
            {
                RunNextStageCommand(callContext, 0);
            }
        }

        public void RunAdHocCommand<TStage, TPayload>(string defaultCommandName, TPayload payload, ICommandDefinition<TStage, TPayload> command)
            where TStage : class
        {
            var commandParams = new CommandParams<TPayload>(defaultCommandName)
            {
                Payload = payload
            };

            var customCommands = Stages.OfType<PipelineStage<TStage>>()
                .ToDictionary(
                    x => x.Config.Name,
                    x => (IStageCommand<TPayload>)new StageCommand<TStage, TPayload>(x.Stage, command));

            var callContext = new StageCallContext<TPayload>(defaultCommandName, commandParams, Stages.Count, customCommands);

            if (callContext.MaxStageIndex > 0)
            {
                RunNextStageCommand(callContext, 0);
            }
        }

        public void StartSession()
        {
            if (SessionStarted == false)
            {
                _sessionStarted = true;
                _sessionAborted = false;
                RunSessionCommand("Start", startSessionCommand, postStartSessionCommand);
            }
        }
        public void AbortSession()
        {
            if (SessionStarted)
            {
                _sessionStarted = false;
                _sessionAborted = true;
                RunSessionCommand("Abort", abortSessionCommand, postAbortSessionCommand);
            }
        }
        public void EndSession()
        {
            if (SessionStarted)
            {
                _sessionStarted = _sessionAborted = false;
                RunSessionCommand("End", endSessionCommand, postEndSessionCommand);
            }
        }

        public bool SessionStarted { get { return _sessionStarted || Stages.Any(s => s.SessionStarted); } }
        public bool SessionAborted { get { return _sessionAborted || Stages.Any(s => s.SessionAborted); } }

        private void RunNextStageCommand<TPayload>(StageCallContext<TPayload> callContext, int stageIndex)
        {
            if (callContext.MostRecentCommandAction == PipelineAction.Abort)
            {
                return;
            }

            var stage = Stages[stageIndex];

            IStageCommand<TPayload> stageCommand;
            callContext.CustomStageCommands.TryGetValue(stage.Config.Name, out stageCommand);
            stageCommand = stageCommand 
                ?? stage.RunCommand<TPayload>(callContext.CommandName) 
                ?? StageCallContext<TPayload>.NullStageCommand;

            callContext.MostRecentCommandAction = stageCommand.RunCommand(callContext.CommandParams);
            switch (callContext.MostRecentCommandAction)
            {
                case PipelineAction.Continue:
                    if (++stageIndex < callContext.MaxStageIndex)
                    {
                        RunNextStageCommand(callContext, stageIndex);
                    }
                    if (callContext.MostRecentCommandAction != PipelineAction.Abort)
                    {
                        callContext.MostRecentCommandAction = stageCommand.RunPostCommand(callContext.CommandParams);
                    }
                    break;
                case PipelineAction.Abort:
                    return;
                case PipelineAction.Stop:
                    {
                        callContext.MostRecentCommandAction = stageCommand.RunPostCommand(callContext.CommandParams);
                    }
                    break;
                default:
                    throw new NotImplementedException(string.Format("Pipeline.RunCommand: PipelineAction {0} not supported", callContext.MostRecentCommandAction));
            }
        }

        private void RunSessionCommand(string typeOfSessionCommand, Action<IPipelineStage> sessionCommand, Action<IPipelineStage> postSessionCommand)
        {
            if (Stages.Any())
            {
                var callContext = new SessionCallContext(sessionCommand, postSessionCommand);
                RunNextSessionCommand(0, callContext);
                if(callContext.Exceptions.Any())
                {
                    throw new AggregateException(string.Format("One or more Stages failed to successfully execute {0}Session", typeOfSessionCommand), callContext.Exceptions);
                }
            }
        }

        private void RunNextSessionCommand(int stageIndex, SessionCallContext callContext)
        {
            var stage = Stages[stageIndex];
            // it's important that all Stages get an opportunity to perform every session command - hence the exception handling here...
            try
            {
                callContext.SessionCommand(stage);
            }
            catch(Exception ex)
            {
                callContext.Exceptions.Add(ex);
            }
            if (++stageIndex < Stages.Count)
            {
                RunNextSessionCommand(stageIndex, callContext);
            }
            try
            {
                callContext.PostSessionCommand(stage);
            }
            catch (Exception ex)
            {
                callContext.Exceptions.Add(ex);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(SessionStarted)
                    {
                        AbortSession();
                    }

                    foreach (var s in Stages)
                    {
                        s.Dispose();
                    }
                }

                Stages = null;
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