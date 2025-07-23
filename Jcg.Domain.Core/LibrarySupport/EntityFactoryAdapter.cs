using System;

namespace Jcg.Domain.Core.LibrarySupport
{
    /// <summary>
    /// An adapter that abstracts the ServiceProvider or any other mechanism that allows the creation of entities with
    /// their dependencies.
    /// </summary>
    public interface IEntityFactoryAdapter
    {
        /*
         * I needed to use an adapter so this lirary can be implemented in netStandard, the IServiceProvider.CreateScope is not available in netStandard.
         */
        object? Create(Type type);
    }
}