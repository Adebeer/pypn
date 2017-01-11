using Pypn.Core.Configuration;
using Pypn.Tests.Mocks;
using System.Collections.Generic;

namespace Pypn.Tests.ConfigTests
{
    public abstract class DefaultConfiguredVisitorsPipelineScenarioTests
    {
        protected const string VisitorA = "Visitor A";
        protected const string VisitorB = "Visitor B";
        public List<string> HistoryOfVisits
        {
            get; private set;
        }

        public IPipelineConfig PipelineConfig
        {
            get; private set;
        }

        protected DefaultConfiguredVisitorsPipelineScenarioTests(string visitCommandName)
        {
            PipelineConfig = new PipelineConfig();
            HistoryOfVisits = new List<string>();
            var visitorA = new StageConfig<Visitor>("V1", () => new Visitor(VisitorA, HistoryOfVisits));
            var visitorB = new StageConfig<Visitor>("V2", () => new Visitor(VisitorB, HistoryOfVisits));

            visitorA.CommandConfig
                .WithVisitCommand(visitCommandName)
                .WithSessionCommands();
            visitorB.CommandConfig
                .WithVisitCommand(visitCommandName)
                .WithSessionCommands();

            PipelineConfig.AddStage(visitorA);
            PipelineConfig.AddStage(visitorB);
        }
    }
}
