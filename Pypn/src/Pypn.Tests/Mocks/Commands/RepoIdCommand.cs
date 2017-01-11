using Pypn.Tests.Mocks.EntityModel;
using System;

namespace Pypn.Tests.Mocks
{
    public interface IRepoIdCommand
    {
        int Id { get; set; }
        Type EntityType { get; }
        IEntity ReturnedItem { get; set; }
    }

    public class RepoIdCommand<TEntity> : IRepoIdCommand where TEntity : IEntity
    { 
        public RepoIdCommand(int id)
        {
            Id = id;
            EntityType = typeof(TEntity);
        }

        public int Id { get; set; }
        public Type EntityType { get; }
        public IEntity ReturnedItem { get { return Entity; } set { Entity = (TEntity)value; } }
        public TEntity Entity { get; private set; }
    }
}