namespace Pypn.Core.Configuration
{
    /// <summary>
    /// Knows how to instantiate a Stage for use in Pipeline
    /// </summary>
    public interface IStageConfig
    {
        string Name { get; }

        IPipelineStage CreateStage();
    }

    public interface IStageConfig<TStage> : IStageConfig
    {
        IStageCommandConfig<TStage> CommandConfig { get; }
    }
}