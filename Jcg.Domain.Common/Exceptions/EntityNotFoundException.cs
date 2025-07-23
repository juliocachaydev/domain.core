using System;

namespace Jcg.Domain.Core.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(Type entityType, Guid id)
        : base($"Entity of type {entityType.Name} with ID {id} was not found.")
        {
            
        }
    }
}