using Pypn.Tests.Mocks.EntityModel;

namespace Pypn.Tests.Mocks
{
    public interface IRepoEntityCommand
    {
        IEntity Entity { get; set; }
    }

    public class RepoEntityCommand<TEntity> : IRepoEntityCommand where TEntity : IEntity
    {
        public RepoEntityCommand(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity
        {
            get; set;
        }
    }
}