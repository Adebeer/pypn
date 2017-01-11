using Pypn.Core.Configuration;
using System;

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
    /// Used internally by framework. It acts as an IDisposable adaptor of custom TStage implementations to facilitate cleanup
    /// and management of TStage instances by an IPipeline instance
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
}