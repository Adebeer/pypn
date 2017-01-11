using Pypn.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pypn.Core.Configuration
{

    /// <summary>
    /// Note: This implementation is only thread-safe once all stages have been added as it seems unecessary (and counterintuitive) to be creating an ordered collection of 
    /// configured Stages from multiple threads. 
    /// </summary>
    public class PipelineConfig : IPipelineConfig
    {
        private readonly List<IStageConfig> _stages = new List<IStageConfig>();

        public IReadOnlyCollection<IStageConfig> ConfiguredStages
        {
            get
            {
                return _stages.AsReadOnly();
            }
        }

        public IPipeline CreatePipeline()
        {
            return new Pipeline(ConfiguredStages);
        }

        public void AddStage<TStage>(IStageConfig<TStage> stageConfig)
        {
            var name = stageConfig.Name;
            if(_stages.Any(x => string.CompareOrdinal(x.Name, name) == 0))
            {
                throw new ArgumentException(string.Format("There is already another configured Stage with name {0}", name));
            }
            _stages.Add(stageConfig);
        }

        public object this[string name]
        {
            get
            {
                return _stages.FirstOrDefault(s => string.CompareOrdinal(s.Name, name) == 0);
            }
        }
    }
}