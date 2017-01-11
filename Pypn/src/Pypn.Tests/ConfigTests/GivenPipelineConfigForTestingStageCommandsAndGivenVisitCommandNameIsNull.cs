using Pypn.Tests.Mocks;
using Xunit;

namespace Pypn.Tests.ConfigTests
{

    public class GivenPipelineConfigForTestingStageCommandsAndGivenVisitCommandNameIsNull : DefaultConfiguredVisitorsPipelineScenarioTests
    {
        public GivenPipelineConfigForTestingStageCommandsAndGivenVisitCommandNameIsNull() : base(visitCommandName : null) { }

        [Fact]
        public void WhenCallingRunCommandWithNullCommandName_ThenVisitCommandCalledInExpectedStageCommandOrder()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.RunCommand(null, "Test");
            }

            Assert.Equal(4, HistoryOfVisits.Count);

            Assert.Collection(HistoryOfVisits,
                x => AssertForVisitor.CalledVisitCommand(VisitorA, "Test", x),
                x => AssertForVisitor.CalledVisitCommand(VisitorB, "Test", x),
                x => AssertForVisitor.CalledPostVisitCommand(VisitorB, "Test", x),
                x => AssertForVisitor.CalledPostVisitCommand(VisitorA, "Test", x));
        }

        [Fact]
        public void WhenCallingRunCommandWithNonExistentCommandName_ThenDefaultVisitCommandCalledInExpectedStageCommandOrder()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                // because the Visitor was defined without specifying command name, it will apply to any command run on pipeline, as long as it matches the command TPayload type
                pipeline.RunCommand("SomeOtherUndefinedCommand", "Test");
            }

            Assert.Equal(4, HistoryOfVisits.Count);

            Assert.Collection(HistoryOfVisits,
                x => AssertForVisitor.CalledVisitCommand(VisitorA, "Test", x),
                x => AssertForVisitor.CalledVisitCommand(VisitorB, "Test", x),
                x => AssertForVisitor.CalledPostVisitCommand(VisitorB, "Test", x),
                x => AssertForVisitor.CalledPostVisitCommand(VisitorA, "Test", x));
        }
    }
}
