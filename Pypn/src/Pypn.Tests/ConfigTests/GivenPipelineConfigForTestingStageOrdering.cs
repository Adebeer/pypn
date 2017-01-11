using Pypn.Core;
using Pypn.Core.Configuration;
using Pypn.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Pypn.Tests.ConfigTests
{
    public class GivenPipelineConfigForTestingStageOrdering
    {
        IPipelineConfig _pipelineConfig;

        public GivenPipelineConfigForTestingStageOrdering()
        {
            _pipelineConfig = new PipelineConfig();
            _pipelineConfig.AddStage(new StageConfig<SimpleLogger>("S1", () => new SimpleLogger()));
            _pipelineConfig.AddStage(new StageConfig<Visitor>("S2", () => new Visitor("Visitor A")));
            _pipelineConfig.AddStage(new StageConfig<Visitor>("S3", () => new Visitor("Visitor B")));
        }

        [Fact]
        public void ThenThereAre3ConfiguredStages()
        {
            Assert.Equal(3, _pipelineConfig.ConfiguredStages.Count);
        }
                
        [Theory]
        [InlineData(0, "S1", typeof(StageConfig<SimpleLogger>))]
        [InlineData(1, "S2", typeof(StageConfig<Visitor>))]
        [InlineData(2, "S3", typeof(StageConfig<Visitor>))]
        public void ThenTheConfiguredStagesAreOrderedCorrectly(int expectedIndex, string expectedName, Type expectedType)
        {
            var actualStageConfig = _pipelineConfig.ConfiguredStages.Skip(expectedIndex).FirstOrDefault();
            Assert.NotNull(actualStageConfig);
            Assert.Equal(expectedName, actualStageConfig.Name);
            Assert.Equal(expectedType, actualStageConfig.GetType());
        }

        [Theory]
        [InlineData(0, "S1", typeof(PipelineStage<SimpleLogger>))]
        [InlineData(1, "S2", typeof(PipelineStage<Visitor>))]
        [InlineData(2, "S3", typeof(PipelineStage<Visitor>))]
        public void WhenCreatingPipelineThenThePipelineHasExpectedStagesInCorrectOrder(int expectedIndex, string expectedName, Type expectedType)
        {
            using (var pipeline = _pipelineConfig.CreatePipeline())
            {
                var actualStage = pipeline.Stages.Skip(expectedIndex).FirstOrDefault();
                Assert.NotNull(actualStage);
                Assert.Equal(expectedName, actualStage.Config.Name);
                Assert.Equal(expectedType, actualStage.GetType());
            }
        }
    }
}
