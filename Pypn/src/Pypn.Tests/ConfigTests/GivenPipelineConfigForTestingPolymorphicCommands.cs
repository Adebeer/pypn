using Pypn.Core;
using Pypn.Core.Configuration;
using Pypn.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pypn.Tests.ConfigTests
{
    public class GivenPipelineConfigForTestingPolymorphicCommands
    {
        private static class StageName
        {
            public const string VisitorA = "Visitor A";
        }

        public static class CommandName
        {
            public const string Default = null;
            public const string X = "X";
        }

        public static class Given {

            public static class CommandDefinition
            {
                private const string prefix = nameof(Given) + nameof(CommandDefinition);
                public const string BasicCommand = prefix + nameof(BasicCommand);
                public const string DerivedCommand = prefix + nameof(DerivedCommand);
                public const string DerivedCommandV2 = prefix + nameof(DerivedCommandV2);
                public const string UnrelatedCommand = prefix + nameof(UnrelatedCommand);

                public static void DefineGivenCommand(IStageCommandConfig<Visitor> stageConfig, string commandName, string commandDefinition)
                {
                    switch (commandDefinition)
                    {
                        case BasicCommand:
                            stageConfig.WithCommand<Visitor, BasicCommand>(commandName,
                                (s, arg) => s.Visit($"{arg.CommandName}:{arg.Payload}"),
                                (s, arg) => s.PostVisit($"{arg.CommandName}:{arg.Payload}"));
                            break;
                        case DerivedCommand:
                            stageConfig.WithCommand<Visitor, DerivedCommand>(commandName,
                                (s, arg) => s.Visit($"{arg.CommandName}:{arg.Payload}"),
                                (s, arg) => s.PostVisit($"{arg.CommandName}:{arg.Payload}"));
                            break;
                        case DerivedCommandV2:
                            stageConfig.WithCommand<Visitor, DerivedCommandV2>(commandName,
                                (s, arg) => s.Visit($"{arg.CommandName}:{arg.Payload}"),
                                (s, arg) => s.PostVisit($"{arg.CommandName}:{arg.Payload}"));
                            break;
                        case UnrelatedCommand:
                            stageConfig.WithCommand<Visitor, UnrelatedCommand>(commandName,
                                                            (s, arg) => s.Visit($"{arg.CommandName}:{arg.Payload}"),
                                                            (s, arg) => s.PostVisit($"{arg.CommandName}:{arg.Payload}"));
                            break;
                        default:
                            throw new NotImplementedException(commandDefinition);
                    }
                }
            }
        }

        public static class CallWith {
            public static class CommandType
            {
                private const string prefix = nameof(CallWith) + nameof(CommandType);
                public const string BasicCommand = prefix + nameof(BasicCommand);
                public const string DerivedCommand = prefix + nameof(DerivedCommand);
                public const string DerivedCommandV2 = prefix + nameof(DerivedCommandV2);
                public const string UnrelatedCommand = prefix + nameof(UnrelatedCommand);

                public static void CallRunCommand(IPipeline pipeline, string commandName, string callWithCommandDefinition)
                {
                    switch (callWithCommandDefinition)
                    {
                        case BasicCommand:
                            pipeline.RunCommand(commandName, CommandValue.BasicCommand());
                            break;
                        case DerivedCommand:
                            pipeline.RunCommand(commandName, CommandValue.DerivedCommand());
                            break;
                        case DerivedCommandV2:
                            pipeline.RunCommand(commandName, CommandValue.DerivedCommandV2());
                            break;
                        case UnrelatedCommand:
                            pipeline.RunCommand(commandName, CommandValue.UnrelatedCommand());
                            break;
                        default:
                            throw new NotImplementedException(callWithCommandDefinition);
                    }
                }
            }

            public static class CommandValue
            {
                public static readonly Func<BasicCommand> BasicCommand = () => new BasicCommand("Test");
                public static readonly Func<DerivedCommand> DerivedCommand = () => new DerivedCommand("Test", "Extra");
                public static readonly Func<DerivedCommandV2> DerivedCommandV2 = () => new DerivedCommandV2("Test", "ExtraV2", 2);
                public static readonly Func<UnrelatedCommand> UnrelatedCommand = () => new UnrelatedCommand("Test");
            }
        }

        public static class Expect
        {
            public static class Invoked
            {
                public const string BasicCommand = nameof(BasicCommand);
                public const string DerivedCommand = nameof(DerivedCommand);
                public const string DerivedCommandV2 = nameof(DerivedCommandV2);
                public const string UnrelatedCommand = nameof(UnrelatedCommand);
                public const string Nothing = nameof(Nothing);

                public static Action<string>[] GetExpectedHistory(string expectedCommand, string expectedCommandName)
                {
                    switch (expectedCommand)
                    {
                        case BasicCommand:
                            return new Action<string>[] {
                                x => AssertForVisitor.CalledVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.BasicCommand()}", x),
                                x => AssertForVisitor.CalledPostVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.BasicCommand()}", x)
                            };
                        case DerivedCommand:
                            return new Action<string>[] {
                                x => AssertForVisitor.CalledVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.DerivedCommand()}", x),
                                x => AssertForVisitor.CalledPostVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.DerivedCommand()}", x)
                            };
                        case DerivedCommandV2:
                            return new Action<string>[] {
                                x => AssertForVisitor.CalledVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.DerivedCommandV2()}", x),
                                x => AssertForVisitor.CalledPostVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.DerivedCommandV2()}", x)
                            };
                        case UnrelatedCommand:
                            return new Action<string>[] {
                                x => AssertForVisitor.CalledVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.UnrelatedCommand()}", x),
                                x => AssertForVisitor.CalledPostVisitCommand(StageName.VisitorA, $"{expectedCommandName}:{CallWith.CommandValue.UnrelatedCommand()}", x)
                            };
                    }
                    return new Action<string>[0];
                }
            }
        }

        private List<string> HistoryOfVisits
        {
            get; set;
        }

        private IPipelineConfig PipelineConfig
        {
            get; set;
        }

        public class BasicCommand
        {
            public BasicCommand(string basicData)
            {
                BasicData = basicData;
            }

            public string BasicData { get; }

            public override string ToString()
            {
                return $"BasicData {BasicData}";
            }
        }

        public class DerivedCommand : BasicCommand
        {
            public DerivedCommand(string basicData, string extraData)
                : base(basicData)
            {
                ExtraData = extraData;
            }

            public string ExtraData { get; }

            public override string ToString()
            {
                return $"BasicData {BasicData}; ExtraData {ExtraData}";
            }
        }

        public class DerivedCommandV2 : DerivedCommand
        {
            public DerivedCommandV2(string basicData, string extraData, int extraIntValue)
                : base(basicData, extraData)
            {
                ExtraIntValue = extraIntValue;
            }

            public int ExtraIntValue { get; }

            public override string ToString()
            {
                return $"BasicData {BasicData}; ExtraData {ExtraData}; ExtraIntValue {ExtraIntValue}";
            }
        }

        public class UnrelatedCommand
        {
            public UnrelatedCommand(string basicData)
            {
                BasicData = basicData;
            }

            public string BasicData { get; }

            public override string ToString()
            {
                return $"BasicData {BasicData}";
            }
        }

        private StageConfig<Visitor> StageConfigVisitorA { get; set; }

        public GivenPipelineConfigForTestingPolymorphicCommands()
        {
            PipelineConfig = new PipelineConfig();
            HistoryOfVisits = new List<string>();
            StageConfigVisitorA = new StageConfig<Visitor>(StageName.VisitorA, () => new Visitor(StageName.VisitorA, HistoryOfVisits));
            PipelineConfig.AddStage(StageConfigVisitorA);
        }

        [Theory]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.X, CallWith.CommandType.BasicCommand, CommandName.X, Expect.Invoked.BasicCommand, CommandName.X)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.X, CallWith.CommandType.BasicCommand, CommandName.Default, Expect.Invoked.Nothing, CommandName.Default)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.X, CallWith.CommandType.DerivedCommand, CommandName.X, Expect.Invoked.BasicCommand, CommandName.X)]

        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.X, CallWith.CommandType.UnrelatedCommand, CommandName.X, Expect.Invoked.Nothing, CommandName.Default)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.UnrelatedCommand, CommandName.Default, Expect.Invoked.Nothing, CommandName.Default)]

        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.BasicCommand, CommandName.X, Expect.Invoked.BasicCommand, CommandName.X)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.BasicCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.DerivedCommand, CommandName.X, Expect.Invoked.BasicCommand, CommandName.X)]
        public void GivenCommandDefinitionsWhenCallWithThenExpected(string givenCommandDefinition, string withGivenCommandName, string whenCallWithCommandType, string andCallWithCommandName, string thenExpectedInvokedCommand, string withExpectedCommandName)
        {
            Given.CommandDefinition.DefineGivenCommand(StageConfigVisitorA.CommandConfig, withGivenCommandName, givenCommandDefinition);

            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                CallWith.CommandType.CallRunCommand(pipeline, andCallWithCommandName, whenCallWithCommandType);
            }
            
            var expectedHistory = Expect.Invoked.GetExpectedHistory(thenExpectedInvokedCommand, withExpectedCommandName);

            Assert.Collection(HistoryOfVisits, expectedHistory.ToArray());
        }

        [Theory]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.DerivedCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.DerivedCommand, CommandName.X, Expect.Invoked.DerivedCommand, CommandName.X)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.BasicCommand, CommandName.X, Expect.Invoked.BasicCommand, CommandName.X)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.DerivedCommandV2, CommandName.X, Expect.Invoked.DerivedCommand, CommandName.X)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.DerivedCommandV2, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]

        
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.BasicCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.DerivedCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.BasicCommand, CommandName.X, Expect.Invoked.BasicCommand, CommandName.X)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.X, CallWith.CommandType.DerivedCommand, CommandName.X, Expect.Invoked.DerivedCommand, CommandName.X)]

        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.Default, CallWith.CommandType.BasicCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.DerivedCommand, CommandName.Default, CallWith.CommandType.DerivedCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]


        // note: we only run basic command once; 2nd/duplicate ignored
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.BasicCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)] 
        [InlineData(Given.CommandDefinition.BasicCommand, CommandName.Default, Given.CommandDefinition.BasicCommand, CommandName.X, CallWith.CommandType.BasicCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]

        [InlineData(Given.CommandDefinition.DerivedCommand, CommandName.Default, Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.BasicCommand, CommandName.Default, Expect.Invoked.BasicCommand, CommandName.Default)]
        [InlineData(Given.CommandDefinition.DerivedCommand, CommandName.Default, Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.DerivedCommand, CommandName.Default, Expect.Invoked.DerivedCommand, CommandName.Default)]
        [InlineData(Given.CommandDefinition.DerivedCommand, CommandName.Default, Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.BasicCommand, CommandName.X, Expect.Invoked.BasicCommand, CommandName.X)]
        [InlineData(Given.CommandDefinition.DerivedCommand, CommandName.Default, Given.CommandDefinition.BasicCommand, CommandName.Default, CallWith.CommandType.DerivedCommand, CommandName.X, Expect.Invoked.DerivedCommand, CommandName.X)]
        public void GivenCommandDefinitionsWhenCallWithThenExpected(string givenCommandDefinition1, string withGivenCommandName1, string givenCommandDefinition2, string withGivenCommandName2, string whenCallWithCommandType, string andCallWithCommandName, string thenExpectedInvokedCommand, string withExpectedCommandName)
        {
            Given.CommandDefinition.DefineGivenCommand(StageConfigVisitorA.CommandConfig, withGivenCommandName1, givenCommandDefinition1);
            Given.CommandDefinition.DefineGivenCommand(StageConfigVisitorA.CommandConfig, withGivenCommandName2, givenCommandDefinition2);

            using (var pipeline = PipelineConfig.CreatePipeline())
            {
                CallWith.CommandType.CallRunCommand(pipeline, andCallWithCommandName, whenCallWithCommandType);
            }

            var expectedHistory = Expect.Invoked.GetExpectedHistory(thenExpectedInvokedCommand, withExpectedCommandName);

            Assert.Collection(HistoryOfVisits, expectedHistory.ToArray());
        }


    }
}
