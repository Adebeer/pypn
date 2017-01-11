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
}