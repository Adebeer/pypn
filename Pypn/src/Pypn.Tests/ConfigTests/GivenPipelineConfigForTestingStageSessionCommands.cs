using Pypn.Tests.Mocks;
using Xunit;

namespace Pypn.Tests.ConfigTests
{
    public class GivenPipelineConfigForTestingStageSessionCommands : DefaultConfiguredVisitorsPipelineScenarioTests
    {
        public GivenPipelineConfigForTestingStageSessionCommands() : base(visitCommandName : null) { }

        [Fact]
        public void WhenStartingSession_ThenStartSessionCalledInExpectedStageCommandOrder()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.StartSession();

                Assert.True(pipeline.SessionStarted);

                Assert.Equal(4, HistoryOfVisits.Count);
                Assert.Collection(HistoryOfVisits,
                    x => AssertForVisitor.CalledStartSession(VisitorA, x),
                    x => AssertForVisitor.CalledStartSession(VisitorB, x),
                    x => AssertForVisitor.CalledPostStartSession(VisitorB, x),
                    x => AssertForVisitor.CalledPostStartSession(VisitorA, x));
            }
        }

        [Fact]
        public void WhenStartingSessionAndDisposingPipeline_ThenSessionAborted()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.StartSession();
            }

            AssertSessionStartedAndAborted();
        }

        [Fact]
        public void WhenStartingAndAbortingSessionAndDisposingPipeline_ThenSessionAbortedOnce()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.StartSession();
                pipeline.AbortSession();
            }

            AssertSessionStartedAndAborted();
        }

        [Fact]
        public void WhenStartingAndEndingSessionAndDisposingPipeline_ThenSessionNotAborted()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.StartSession();
                Assert.True(pipeline.SessionStarted);
                Assert.False(pipeline.SessionAborted);
                pipeline.EndSession();
                Assert.False(pipeline.SessionStarted);
                Assert.False(pipeline.SessionAborted);
            }

            Assert.Equal(8, HistoryOfVisits.Count);

            Assert.Collection(HistoryOfVisits,
                x => AssertForVisitor.CalledStartSession(VisitorA, x),
                x => AssertForVisitor.CalledStartSession(VisitorB, x),
                x => AssertForVisitor.CalledPostStartSession(VisitorB, x),
                x => AssertForVisitor.CalledPostStartSession(VisitorA, x),
                x => AssertForVisitor.CalledEndSession(VisitorA, x),
                x => AssertForVisitor.CalledEndSession(VisitorB, x),
                x => AssertForVisitor.CalledPostEndSession(VisitorB, x),
                x => AssertForVisitor.CalledPostEndSession(VisitorA, x)
                );
        }

        [Fact]
        public void WhenNotStartingSessionAndDisposingPipeline_ThenNoSessionStartedOrAborted()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                // do nothing
            };

            Assert.Equal(0, HistoryOfVisits.Count);
        }

        [Fact]
        public void WhenStartingSessionTwice_ThenSessionOnlyStartedOnce()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.StartSession();
                pipeline.StartSession();

                Assert.True(pipeline.SessionStarted);

                Assert.Equal(4, HistoryOfVisits.Count);
                Assert.Collection(HistoryOfVisits,
                    x => AssertForVisitor.CalledStartSession(VisitorA, x),
                    x => AssertForVisitor.CalledStartSession(VisitorB, x),
                    x => AssertForVisitor.CalledPostStartSession(VisitorB, x),
                    x => AssertForVisitor.CalledPostStartSession(VisitorA, x));
            }

            Assert.Equal(8, HistoryOfVisits.Count); // and it has been aborted
        }

        [Fact]
        public void WhenEndingSessionWithoutStartingIt_ThenNoSessionCommandsInvoked()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.EndSession();
                Assert.False(pipeline.SessionStarted);
                Assert.False(pipeline.SessionAborted);
            }

            Assert.Equal(0, HistoryOfVisits.Count);
        }

        [Fact]
        public void WhenAbortingSessionWithoutStartingIt_ThenNoSessionCommandsInvoked()
        {
            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                pipeline.AbortSession();
                Assert.False(pipeline.SessionStarted);
                Assert.False(pipeline.SessionAborted);
            }

            Assert.Equal(0, HistoryOfVisits.Count);
        }

        private void AssertSessionStartedAndAborted()
        {
            Assert.Equal(8, HistoryOfVisits.Count);

            Assert.Collection(HistoryOfVisits,
                x => AssertForVisitor.CalledStartSession(VisitorA, x),
                x => AssertForVisitor.CalledStartSession(VisitorB, x),
                x => AssertForVisitor.CalledPostStartSession(VisitorB, x),
                x => AssertForVisitor.CalledPostStartSession(VisitorA, x),
                x => AssertForVisitor.CalledAbortSession(VisitorA, x),
                x => AssertForVisitor.CalledAbortSession(VisitorB, x),
                x => AssertForVisitor.CalledPostAbortSession(VisitorB, x),
                x => AssertForVisitor.CalledPostAbortSession(VisitorA, x)
                );
        }
    }
}