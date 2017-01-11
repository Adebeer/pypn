using Pypn.Tests.Mocks.EntityModel;
using System;
using System.Collections.Generic;

namespace Pypn.Tests.Mocks
{
    public class Repository
    {
        private class EntityId : IEquatable<EntityId>
        {
            public int Id { get; set; }
            public string EntityType { get; set; }

            public EntityId(int id, string entityType)
            {
                Id = id;
                EntityType = entityType;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as EntityId);
            }

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 29 + Id.GetHashCode();
                hash = hash * 29 + (EntityType?.GetHashCode() ?? 0);
                return hash;
            }

            public bool Equals(EntityId other)
            {
                if(other == null)
                {
                    return false;
                }
                return other.Id == Id && string.CompareOrdinal(other.EntityType, EntityType) == 0;
            }
        }

        private Dictionary<EntityId, IEntity> _entities = new Dictionary<EntityId, IEntity>();
        private int NextId = 0;

        public IEntity GetValue(int id, Type entityType)
        {
            var key = new EntityId(id, entityType.Name);
            IEntity entity;
            _entities.TryGetValue(key, out entity);
            return entity;
        }

        public T GetValue<T>(int id) where T : IEntity
        {
            var key = new EntityId(id, typeof(T).Name);
            IEntity entity;
            _entities.TryGetValue(key, out entity);
            return (T) entity;
        }

        public void Save(IEntity entity)
        {
            if (entity.Id == 0)
            {
                entity.Id = ++NextId;
                var key = new EntityId(entity.Id, entity.GetType().Name);
                _entities[key] = entity;
            }
        }

        public void Delete(IEntity entity) 
        {
            if(entity.Id > 0)
            {
                var key = new EntityId(entity.Id, entity.GetType().Name);
                _entities.Remove(key);
                entity.Id = 0;
            }
        }
    }
}