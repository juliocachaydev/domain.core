using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.Repository;

namespace Jcg.Domain.Core.LibrarySupport
{
    /// <summary>
    ///  A container to store the types of IEntityStrategies available in the app.
    /// </summary>
    public class RepositoryStrategyCollection
    {
        private readonly Assembly[] _assembliesToScan;

        // Key is the entity type, value is the strategy type
        private Dictionary<Type, Type> _strategies = new Dictionary<Type, Type>();

        public RepositoryStrategyCollection(Assembly[] assembliesToScan)
        {
            _assembliesToScan = assembliesToScan;
        }

        /// <summary>
        /// Scans the provided assemblies for implementations of IEntityStrategy, and stores them internall so they are
        /// available for later use. Call this method once, when your application starts. This class should be available as a singleton
        /// </summary>
        /// <param name="sp">The service provider is necessary because the types for which an IEntityStrategy applies are defined as a property
        /// (see Types array), to read that property this library must create an instance of that implementation</param>
        /// <param name="assembliesToScan"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ScanForStrategies(IEntityFactoryAdapter factory)
        {
            if (_strategies.Any())
            {
                return;
            }

            var types = _assembliesToScan.SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass && x.GetInterfaces().Any())
                .Where(x => x.GetInterfaces().Contains(typeof(IEntityStrategy)))
                .ToArray();

            foreach (var t in types)
            {
                var obj = factory.Create(t);

                if (obj is null)
                {
                    throw new EntityStrategyNotFoundException(t);
                }

                var instance = (IEntityStrategy)factory.Create(t)!;

                foreach (var entityType in instance.EntityTypes)
                {
                    _strategies.Add(entityType, t);
                }
            }

        }

        /// <summary>
        /// Given a scope, this method creates an instance of the IEntityStrategy or throws an exception if the entityType does not
        /// correspond to any of the IEntityStrategy found by the ScanForStrategies method.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEntityStrategy CreateStrategyOrThrow(IEntityFactoryAdapter factory, Type entityType)
        {
            if (_strategies.TryGetValue(entityType, out var strategyType))
            {
                var obj = factory.Create(strategyType);

                if (obj is null)
                {
                    throw new EntityStrategyNotFoundException(entityType);
                }

                var instance = (IEntityStrategy)factory.Create(strategyType)!;

                return instance;
            }

            throw new EntityStrategyNotFoundException(entityType);
        }
    }
}