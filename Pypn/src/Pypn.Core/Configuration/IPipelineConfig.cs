using System.Collections.Generic;

namespace Pypn.Core.Configuration
{
    public interface IPipelineConfig
    {
        IReadOnlyCollection<IStageConfig> ConfiguredStages { get; }

        IPipeline CreatePipeline();

        /// <summary>
        /// Returns stage with given name, otherwise null. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object this[string name] { get; }

        void AddStage<TStage>(IStageConfig<TStage> stageConfig);
    }
}