using System;
using System.Linq;
using System.Threading.Tasks;
using Jcg.Domain.Core.Domain;
using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.LibrarySupport;

namespace Jcg.Domain.Core.Repository;

/// <summary>
/// A Repository to abstract storage operations
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Adds an aggregate but does not commit it.
    /// </summary>
    Task AddAsync<T>(T entity) where T : class;

    /// <summary>
    /// Loads an aggregate by Id. If the aggregate does not exist, returns null. Tracks changes.
    /// </summary>
    Task<T?> LoadAsync<T>(Guid id) where T : class;

    // <summary>
    /// Loads an aggregate by Id. If the aggregate does not exist, throws an exception. Tracks changes
    /// </summary>
    Task<T> LoadOrThrowAsync<T>(Guid id) where T : class;

    /// <summary>
    /// Removes an aggregate by Id. If the aggregate does not exist, does nothing. Does not commit changes.
    /// </summary>
    Task RemoveAsync<T>(Guid id) where T : class;

    /// <summary>
    /// Commits changes in a single transaction.
    /// </summary>
    Task CommitChangesAsync();

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

        private IEntityStrategy CreateStrategy(Type entityType)
        {
            return _strategyCollection.CreateStrategyOrThrow(_entityFactoryAdapter, entityType);
        }

        public async Task AddAsync<T>(T entity) where T : class
        {
            await CreateStrategy(typeof(T)).AddAsync(entity);
        }

        public async Task<T?> LoadAsync<T>(Guid id) where T : class
        {
            var s = CreateStrategy(typeof(T));

            var result = await s.LoadAsync<T>(id);

            if (result is null) return null;

            return (result as T)!;
        }

        public async Task<T> LoadOrThrowAsync<T>(Guid id) where T : class
        {
            var result = await LoadAsync<T>(id);

            if (result is null) throw new EntityNotFoundException(typeof(T), id);
            return result;
        }

        public async Task RemoveAsync<T>(Guid id) where T : class
        {
            var s = CreateStrategy(typeof(T));

            var entity = await LoadAsync<T>(id);

            if (entity != null) s.RemoveAsync(entity);
        }

        public async Task CommitChangesAsync()
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

            foreach (var e in aggregates) e.AssertEntityStateIsValid();

            await _databaseAdapter.SaveChangesAsync();
        }
    }
}