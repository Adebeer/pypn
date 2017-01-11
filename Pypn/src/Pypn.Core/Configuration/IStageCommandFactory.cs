
namespace Pypn.Core.Configuration
{
    public interface IStageCommandFactory<in TStage>
    {
        IStageCommand CreateForStage(TStage stage);

        ICommandDefinition<TStage> StageCommandDefinition { get; }
    }

    public interface IStageCommandFactory<in TStage, in TPayload> : IStageCommandFactory<TStage>
    {
        ICommandDefinition<TStage, TPayload> CommandDefinition { get; }
    }
}
