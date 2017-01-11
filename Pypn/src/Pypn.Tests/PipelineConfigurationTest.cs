using System;
using Pypn.Core;
using Pypn.Core.Configuration;
using Pypn.Tests.Mocks;
using Xunit;
using Pypn.Tests.Mocks.EntityModel;

namespace Pypn.Tests
{
    public class CrudExampleScenarioTest
    {
        public static class Commands
        {
            public const string Get = "Get";
            public const string Save = "Save";
            public const string Delete = "Delete";
            public const string Special = "Special";
        }


        [Fact]
        public void BasicPipeline()
        {
            var configuredPipeline = ConfigurePipeline();

            using (var pipeline = configuredPipeline.CreatePipeline())
            {
                var getByIdRequest = new RepoIdCommand<Square>(1);
                pipeline.RunCommand<IRepoIdCommand>(Commands.Get, getByIdRequest);
                Assert.True(getByIdRequest.ReturnedItem == null, "Repository Get should return null for non-saved item");

                var repoEntityCommand = new RepoEntityCommand<Square>(new Square(10));
                Assert.True(repoEntityCommand.Entity.Id == 0, "PreCondition: Square has no Id and doesn't exist in database");
                pipeline.RunCommand<IRepoEntityCommand>(Commands.Save, repoEntityCommand);
                Assert.True(repoEntityCommand.Entity.Id > 0, "Repository Save should update saved item with Id > 0");

                var id = repoEntityCommand.Entity.Id;
                var getByIdRequest1 = new RepoIdCommand<Square>(id);
                pipeline.RunCommand<IRepoIdCommand>(Commands.Get, getByIdRequest1);
                Assert.True(getByIdRequest1.Entity.Equals(repoEntityCommand.Entity), "Repository Get should return saved item");

                var getByIdRequest2 = new RepoIdCommand<Square>(id);
                pipeline.RunCommand(Commands.Get, getByIdRequest2);
                Assert.True(getByIdRequest2.Entity.Equals(repoEntityCommand.Entity), "Repository Get should return saved item");

                pipeline.RunCommand<IRepoEntityCommand>(Commands.Delete, repoEntityCommand);
                Assert.True(repoEntityCommand.Entity.Id == 0, "Repository Delete should reset Id back to default of 0");

                pipeline.RunCommand<IRepoIdCommand>(Commands.Get, getByIdRequest1);
                Assert.True(getByIdRequest1.Entity == null, "Repository Get should return null for non-saved item");
            }
        }

        protected IPipelineConfig ConfigurePipeline()
        {
            IPipelineConfig pipelineConfig = new PipelineConfig();
            IStageConfig<SimpleLogger> loggerStageConfig = new StageConfig<SimpleLogger>("Logger", () => new SimpleLogger());
            IStageConfig<Repository> repositoryStageConfig = new StageConfig<Repository>("MemoryDb", () => new Repository());
            DefineSimpleLoggerCommands(loggerStageConfig);
            DefineRepositoryCommands(repositoryStageConfig);

            pipelineConfig.AddStage(loggerStageConfig);
            pipelineConfig.AddStage(repositoryStageConfig);
            //todo: add stages to, for example, provide level 1 or level 2 caching solutions
            return pipelineConfig;
        }

        private void DefineSimpleLoggerCommands(IStageConfig<SimpleLogger> stage)
        {
            var idCommand = new CommandDefinition<SimpleLogger, IRepoIdCommand>(
                            (s, arg) => s.Debug($"Command: {arg.CommandName}, Args: {arg.Payload.Id}, Item: {arg.Payload.ReturnedItem}"),
                            (s, arg) => s.Debug($"PostCommand: {arg.CommandName}, Args: {arg.Payload.Id}, Item: {arg.Payload.ReturnedItem}"));

            var entityCommand = new CommandDefinition<SimpleLogger, IRepoEntityCommand>(
                (s, arg) => s.Debug($"Command: {arg.CommandName}, Args: {arg.Payload.Entity.Id}"),
                (s, arg) => s.Debug($"PostCommand: {arg.CommandName}, Args: {arg.Payload.Entity.Id}"));

            var specialCommand = new CommandDefinition<SimpleLogger, IRepoEntityCommand>(
                (s, arg) => s.Debug($"Special Command: {arg.CommandName}, Args: {arg.Payload.Entity.Id}"),
                (s, arg) => s.Debug($"Special PostCommand: {arg.CommandName}, Args: {arg.Payload.Entity.Id}"));

            // command name of null implies it will be executed by default for all commands (where no specific command is defined)
            stage.CommandConfig
                .WithCommand(null, idCommand)
                .WithCommand(null, entityCommand)
                .WithCommand(Commands.Special, specialCommand);
        }

        private void DefineRepositoryCommands(IStageConfig<Repository> stage)
        {
            var getCommand = new CommandDefinition<Repository, IRepoIdCommand>(
                            (s, arg) =>
                            {
                                arg.Payload.ReturnedItem = s.GetValue(arg.Payload.Id, arg.Payload.EntityType);
                                return arg.Payload.ReturnedItem == null ? PipelineAction.Stop : PipelineAction.Continue;
                            });

            var saveCommand = new CommandDefinition<Repository, IRepoEntityCommand>(
                            (s, arg) => {
                                s.Save(arg.Payload.Entity);
                            });

            var deleteCommand = new CommandDefinition<Repository, IRepoEntityCommand>(
                            (s, arg) =>
                            {
                                s.Delete(arg.Payload.Entity);
                            });

            stage.CommandConfig
                .WithCommand(Commands.Get, getCommand)
                .WithCommand(Commands.Save, saveCommand)
                .WithCommand(Commands.Delete, deleteCommand);
        }
    }
}