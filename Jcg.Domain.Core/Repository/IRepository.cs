using System;
using System.Linq;
using System.Threading.Tasks;
using Jcg.Domain.Core.Domain;
using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.LibrarySupport;

namespace Jcg.Domain.Core.Repository
{
    /// <summary>
    /// A Repository to abstract storage operations
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Adds an aggregate but does not commit it.
        /// </summary>
        Task Add<T>(T entity) where T : class;

        /// <summary>
        /// Loads an aggregate by Id. If the aggregate does not exist, returns null. Tracks changes.
        /// </summary>
        Task<T?> Load<T>(Guid id) where T : class;

        // <summary>
        /// Loads an aggregate by Id. If the aggregate does not exist, throws an exception. Tracks changes
        /// </summary>
        Task<T> LoadOrThrow<T>(Guid id) where T : class;

        /// <summary>
        /// Removes an aggregate by Id. If the aggregate does not exist, does nothing. Does not commit changes.
        /// </summary>
        Task Remove<T>(Guid id) where T : class;

        /// <summary>
        /// Commits changes in a single transaction.
        /// </summary>
        Task CommitChanges();

        internal class Imp : IRepository
        {
            private readonly IDatabaseAdapter _databaseAdapter;
            private readonly RepositoryStrategyCollection _strategyCollection;
            private readonly IDomainEventDispatcher _dispatcher;
            private readonly IEntityFactoryAdapter _entityFactoryAdapter;

            public Imp(
                IDatabaseAdapter databaseAdapter, 
                RepositoryStrategyCollection strategyCollection,
                IDomainEventDispatcher dispatcher, 
                IEntityFactoryAdapter entityFactoryAdapter)
            {
                _databaseAdapter = databaseAdapter;
                _strategyCollection = strategyCollection;
                _strategyCollection.ScanForStrategies(entityFactoryAdapter);
                _dispatcher = dispatcher;
                _entityFactoryAdapter = entityFactoryAdapter;
            }

            private IEntityStrategy CreateStrategy(Type entityType) => _strategyCollection.CreateStrategyOrThrow(_entityFactoryAdapter, entityType);
            
            public async Task Add<T>(T entity) where T : class
            {
                await CreateStrategy(typeof(T)).Add(entity);
            }

            public async Task<T?> Load<T>(Guid id) where T : class
            {
                var s = CreateStrategy(typeof(T));
            
                var result = await s.Load<T>(id);

                if (result is null)
                {
                    return null;
                }

                return (result as T)!;

            }

            public async Task<T> LoadOrThrow<T>(Guid id) where T : class
            {
                var result = await Load<T>(id);

                if (result is null)
                {
                    throw new EntityNotFoundException(typeof(T), id);
                }
                return result;

            }

            public async Task Remove<T>(Guid id) where T : class
            {
                var s = CreateStrategy(typeof(T));

                var entity = await Load<T>(id);

                if (entity != null)
                {
                    s.Remove(entity);
                }

            }

            public async Task CommitChanges()
            {
                // For aggregates, we dispatch domain events, and then assert invariants.
                var aggregates = _databaseAdapter.GetTrackedEntities()
                    .Where(e => e is AggregateBase)
                    .Cast<AggregateBase>()
                    .ToArray();
                
                foreach (var e in aggregates)
                {
                    var domainEvents = e.DomainEvents.ToList();
                    foreach (var domainEvent in domainEvents)
                    {
                        // We need to remove the domain event because if a domain event handlers calls commit on the repository again, we will run into an infinite loop.
                        e.RemoveDomainEvent(domainEvent);
                        await _dispatcher.DispatchAsync(domainEvent);
                        
                    }
                }
                foreach (var e in aggregates)
                {
                    e.AssertEntityStateIsValid();
                }

                await _databaseAdapter.SaveChangesAsync();
            }
        }
    }
}