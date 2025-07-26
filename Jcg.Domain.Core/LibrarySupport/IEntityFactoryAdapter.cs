using System;

namespace Jcg.Domain.Core.LibrarySupport;

/// <summary>
/// An adapter that abstracts the ServiceProvider or any other mechanism that allows the creation of entities with
/// their dependencies.
/// </summary>
public interface IEntityFactoryAdapter
{
    /// <summary>
    /// Creates an Scoped instance of a type, resolving its dependencies from the DI Container.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    object? Create(Type type);
}