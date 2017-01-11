using Pypn.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pypn.Core.Framework
{
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