using Pypn.Core.Framework;
using System;

namespace Pypn.Core.Configuration
{
    /// <summary>
    /// A default implementation of IStageConfig{TStage}.
    /// </summary>
    /// <typeparam name="TStage"></typeparam>
    public class StageConfig<TStage> : IStageConfig<TStage> 
        where TStage : class
    {
        private readonly Func<TStage> _stageFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of Stage - must be unique within the context of a configured pipeline. If null, will use <code>typeof(TStage).FullName</code></param>
        /// <param name="stageFactory"></param>
        /// <param name="commandConfig"></param>
        public StageConfig(string name, Func<TStage> stageFactory, IStageCommandConfig<TStage> commandConfig = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? typeof(TStage).FullName : name;
            _stageFactory = stageFactory;
            CommandConfig = commandConfig ?? new StageCommandConfig<TStage>();
        }

        public string Name
        {
            get;
        }

        public IStageCommandConfig<TStage> CommandConfig { get; }

        public IPipelineStage CreateStage()
        {
            return new PipelineStage<TStage>(this, _stageFactory());
        }
    }
}