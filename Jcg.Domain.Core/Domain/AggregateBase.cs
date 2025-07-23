using System;
using System.Collections.Generic;

namespace Jcg.Domain.Core.Domain
{
    /// <summary>
    /// Base class for an aggregate root.
    /// </summary>
    public abstract class AggregateBase
    {
        // Using a backing field here will prevent some ORMS like Ef Core from adding the DomainEvents to the database model, which would be
        // the case if I just used a property.
        private List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        
        public void AddDomainEvent(IDomainEvent ev)
        {
            _domainEvents.Add(ev);
        }
        
        public void RemoveDomainEvent(IDomainEvent ev)
        {
            if (_domainEvents.Contains(ev))
            {
                _domainEvents.Remove(ev);
            }
        }

        /// <summary>
        /// The IRepository calls this method before saving changes. Use it to validate invariants.
        /// If this method throws an exception, the repository will not commit the transaction.
        /// </summary>
        public abstract void AssertEntityStateIsValid();
    }
    
}