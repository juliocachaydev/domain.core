using System;
using System.Threading.Tasks;

namespace Jcg.Domain.Core.Repository
{
    /// <summary>
    /// Abstracts how to operate on the underlying storage for the specified types.
    /// </summary>
    public interface IEntityStrategy
    {
        /// <summary>
        /// You can expose an entity as a class or as many interfaces as needed by adding those types to this array. As long as this
        /// interface is implemented and works for each of the specified types, the repository will also work for those types. This allows you
        /// to implement the interface segregation principle. For instance, when you load an aggregate as a given interface, the underlying object it
        /// the aggregate, but is delivered to you as an interface that exposes only certain members. An advantage of this approach is testing. You can mock
        /// the repo and the types it returns, for instance, an interface.
        /// </summary>
        Type[] EntityTypes { get; }
    
        /// <summary>
        /// Adds an aggregate but does not commit it.
        /// </summary>
        Task AddAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Loads an aggregate by Id. If the aggregate does not exist, returns null. Tracks changes.
        /// </summary>
        Task<TEntity?> LoadAsync<TEntity>(Guid id) where TEntity : class;

        /// <summary>
        /// Removes an aggregate by Id. If the aggregate does not exist, does nothing. Does not commit changes.
        /// </summary>
        void RemoveAsync<TEntity>(TEntity entity) where TEntity : class;
    }
}