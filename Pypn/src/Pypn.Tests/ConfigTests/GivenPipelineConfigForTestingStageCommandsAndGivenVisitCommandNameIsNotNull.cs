using Pypn.Tests.Mocks;
using Xunit;

namespace Pypn.Tests.ConfigTests
{
    /// <summary>
    /// If the Visitor command is defined with a non-null commandName, it is non-default and as such will only apply when explicitly invoked with that command name
    /// </summary>
    public class GivenPipelineConfigForTestingStageCommandsAndGivenVisitCommandNameIsNotNull : DefaultConfiguredVisitorsPipelineScenarioTests
    {
        private const string CommandName = "Visit";

        public GivenPipelineConfigForTestingStageCommandsAndGivenVisitCommandNameIsNotNull() : base(CommandName) { }

        [Fact] 
        public void WhenCallingRunCommandWithNullCommandName_ThenNoCommandCalled() 
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.RunCommand(null, "Test");
            }

            Assert.Equal(0, HistoryOfVisits.Count);
        }

        [Fact]
        public void WhenCallingRunCommandWithUndefinedCommandName_ThenNoCommandCalled()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.RunCommand("SomeUndefinedCommandName", "Test");
            }

            Assert.Equal(0, HistoryOfVisits.Count);
        }

        [Fact]
        public void WhenCallingRunCommandWithGivenCommandName_ThenDefaultVisitCommandCalledInExpectedStageCommandOrder()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                // because the Visitor was defined without specifying command name, it will apply to any command run on pipeline, as long as it matches the command TPayload type
                pipeline.RunCommand(CommandName, "Test");
            }

            Assert.Equal(4, HistoryOfVisits.Count);

            Assert.Collection(HistoryOfVisits,
                x => AssertForVisitor.CalledVisitCommand(VisitorA, "Test", x),
                x => AssertForVisitor.CalledVisitCommand(VisitorB, "Test", x),
                x => AssertForVisitor.CalledPostVisitCommand(VisitorB, "Test", x),
                x => AssertForVisitor.CalledPostVisitCommand(VisitorA, "Test", x));
        }

        //TODO: Test for polymorphic commands

        //Test for different pipeline actions
        //Other unit tests??
    }
}
