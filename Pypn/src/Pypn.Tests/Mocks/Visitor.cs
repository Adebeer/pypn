using Pypn.Core;
using Pypn.Core.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Pypn.Tests.Mocks
{
    /// <summary>
    /// Simple stage implementation that makes it easy to evaluate how Pipeline interacts with stages
    /// </summary>
    public class Visitor
    {
        private readonly List<string> _history;

        

        public Visitor(string nameOfVisitor = null, List<string> history = null)
        {
            VisitorName = nameOfVisitor ?? "Default";
            _history = history ?? new List<string>();
        }

        public string VisitorName { get; }

        public void Visit(string data)
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; Command: {nameof(Visit)}; Data: {data}");
        }

        public void PostVisit(string data)
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; Command: {nameof(PostVisit)}; Data: {data}");
        }

        public IReadOnlyCollection<string> GetCallHistory()
        {
            return _history.AsReadOnly();
        }

        public void StartSession()
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; StartSession called");
        }

        public void PostStartSession()
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; PostStartSession called");
        }

        public void AbortSession()
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; AbortSession called");
        }

        public void PostAbortSession()
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; PostAbortSession called");
        }

        public void EndSession()
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; EndSession called");
        }

        public void PostEndSession()
        {
            _history.Add($"{nameof(Visitor)}: {VisitorName}; PostEndSession called");
        }
    }

    public static class VisitorCommandsHelper
    {
        public static IStageCommandConfig<Visitor> WithVisitCommand(this IStageCommandConfig<Visitor> commandConfig, string commandName, PipelineAction actionAfterVisit = PipelineAction.Continue, PipelineAction actionAfterPostVisit = PipelineAction.Continue)
        {
            var stringCommand = new CommandDefinition<Visitor, string>(
                            (s, arg) => { s.Visit(arg.Payload); return actionAfterVisit; },
                            (s, arg) => { s.PostVisit(arg.Payload); return actionAfterPostVisit; });

            commandConfig
                .WithCommand(commandName, stringCommand);

            return commandConfig;
        }

        public static IStageCommandConfig<Visitor> WithSessionCommands(this IStageCommandConfig<Visitor> commandConfig)
        {
            commandConfig
                .WithStartSessionCommand(s => s.StartSession(), s => s.PostStartSession())
                .WithAbortSessionCommand(s => s.AbortSession(),  s => s.PostAbortSession())
                .WithEndSessionCommand(s => s.EndSession(), s => s.PostEndSession());

            return commandConfig;
        }

    }

    public static class AssertForVisitor
    {
        public static void CalledVisitCommand(string expectedVisitor, string expectedData, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; Command: Visit; Data: {expectedData}", actualVisitorInvocationData);
        }

        public static void CalledPostVisitCommand(string expectedVisitor, string expectedData, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; Command: PostVisit; Data: {expectedData}", actualVisitorInvocationData);
        }

        public static void CalledStartSession(string expectedVisitor, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; StartSession", actualVisitorInvocationData);
        }

        public static void CalledPostStartSession(string expectedVisitor, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; PostStartSession", actualVisitorInvocationData);
        }

        public static void CalledAbortSession(string expectedVisitor, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; AbortSession", actualVisitorInvocationData);
        }

        public static void CalledPostAbortSession(string expectedVisitor, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; PostAbortSession", actualVisitorInvocationData);
        }

        public static void CalledEndSession(string expectedVisitor, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; EndSession", actualVisitorInvocationData);
        }

        public static void CalledPostEndSession(string expectedVisitor, string actualVisitorInvocationData)
        {
            Assert.Contains($"{nameof(Visitor)}: {expectedVisitor}; PostEndSession", actualVisitorInvocationData);
        }
    }
}
