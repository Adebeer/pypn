namespace Pypn.Core.Configuration
{

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
}