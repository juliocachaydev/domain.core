using System;

namespace Jcg.Domain.Core.Exceptions
{
    /// <summary>
    /// An entity was not found in the repository.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(Type entityType, Guid id)
        : base($"Entity of type {entityType.Name} with ID {id} was not found.")
        {
            
        }
    }
}