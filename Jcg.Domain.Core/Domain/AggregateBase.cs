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
        
        /// <summary>
        /// Adds a domain evnet, which will be dispatched by the repository when you call CommitChanges but
        /// before the transaction is committed.
        /// </summary>
        /// <param name="ev"></param>
        public void AddDomainEvent(IDomainEvent ev)
        {
            _domainEvents.Add(ev);
        }
        
        /// <summary>
        /// Removes a domain event. This is called by the Library after a domain event has been dispatched so it can't be dispatched again.
        /// </summary>
        public void RemoveDomainEvent(IDomainEvent ev)
        {
            if (_domainEvents.Contains(ev))
            {
                _domainEvents.Remove(ev);
            }
        }

        /// <summary>
        /// Clears all domain events. This is normally used for testing. In some cases, in the context of integration tests, you would use this method
        /// to prevent side effects caused by domain events.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        /// <summary>
        /// The IRepository calls this method before saving changes. Use it to validate invariants.
        /// If this method throws an exception, the repository will not commit the transaction.
        /// </summary>
        public abstract void AssertEntityStateIsValid();
    }
    
}