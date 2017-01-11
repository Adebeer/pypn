using System;

namespace Pypn.Core.Configuration
{
    /// <summary>
    /// Provides a definition of a particular command implementation. This command is specific to a TStage implementation
    /// </summary>
    public interface ICommandDefinition<in TStage> 
    {
    }

    /// <summary>
    /// A pipeline consists of one or more pipeline stages, each stage (TStage) providing a custom implementation of some functionality. 
    /// 
    /// Implementations of this interface represents an implementation/definition of a particular command/operation 
    /// to run on a TStage implementation supports. In effect, it acts as an adaptor for a command to run on a given instance of TStage
    /// 
    /// When a Command is run, the RunCommand associated with each TStage implementation will be executed on each stage (or until PipelineAction aborts/stops further traversal of stages)
    /// Once the RunCommand has been run on the last stage, RunPostCommand is then run on each of the former stages, but in REVERSE order. 
    /// 
    /// As such, you can view a pipeline as a collection of ordered stages - these stages act like a call stack where RunCommand gets run going down the stack, and RunPostCommand gets
    /// run going back up the stack in reverse order.
    /// </summary>
    /// <typeparam name="TStage">An implementation that forms part of a pipeline. A pipeline consists of one or more (typically heterogeneous) TStage implementations</typeparam>
    /// <typeparam name="TPayload">The parameters/values to be passed to the operation</typeparam>
    public interface ICommandDefinition<in TStage, in TPayload> : ICommandDefinition<TStage>
    {
        /// <summary>
        /// Runs the defined TStage command using the given stage instance.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="callContext"></param>
        /// <returns></returns>
        PipelineAction RunCommand(TStage stage, ICommandParams<TPayload> callContext);

        /// <summary>
        /// Runs the defined TStage Post command using the given stage instance.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="callContext"></param>
        /// <returns></returns>
        PipelineAction RunPostCommand(TStage stage, ICommandParams<TPayload> callContext);
    }

    /// <summary>
    /// Some TStage implementation needs the concept of a session in order to perform its job. For example, a TStage implementation
    /// could be a database repository for a particular database. In this case, calling StartSession() on the pipeline will call
    /// the registered ISessionCommandDefinition associated with the repository/TStage thus allowing it to open a database connection 
    /// (and possibly also start a transaction, if that makes sense from application's perspective).
    /// 
    /// Each TStage has its own ISessionCommandDefinitions{TStage} command for Start, Abort and End Session commands. 
    /// Just like for ICommandDefinition, the RunCommand is called for each pipeline stage. Once all stages are visited, RunPostCommand 
    /// is called in reverse pipeline stage order.
    /// 
    /// Unlike ICommandDefinition, session commands cannot have parameters (TPayload). TStage implementations are typically not stateless and as such
    /// they can be configured to store/recall any information required in order to perform start/abort/end session operations.
    /// </summary>
    /// <typeparam name="TStage"></typeparam>
    public interface ISessionCommandDefinition<in TStage> : ICommandDefinition<TStage>
    {
        PipelineAction RunCommand(TStage stage);
        PipelineAction RunPostCommand(TStage stage);
    }

    /// <summary>
    /// A default implementation of ICommandDefinition however, if needed, clients of this framework can provide custom implementations of this to avoid overhead of calling delegates
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

    /// <summary>
    /// A default implementation of ISessionCommandDefinition however, if needed, clients of this framework can provide custom implementations of this to avoid overhead of calling delegates
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