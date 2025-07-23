using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jcg.Domain.Core.Repository
{
    /// <summary>
    /// An abstraction of the database used to get tracked entities and commit tracked changes on a single transaction.
    /// </summary>
    public interface IDatabaseAdapter
    {
        /// <summary>
        /// Gets tracked entities.
        /// </summary>
        ICollection<object> GetTrackEntities();

        /// <summary>
        /// commits tracked changes in a single transaction.
        /// </summary>
        Task SaveChangesAsync();
    }
}