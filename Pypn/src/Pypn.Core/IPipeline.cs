using Pypn.Core.Configuration;
using System;
using System.Collections.Generic;

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
}