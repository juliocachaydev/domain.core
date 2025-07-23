using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jcg.Domain.Core.Domain;
using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.LibrarySupport;
using Jcg.Domain.Core.Repository;

namespace Jcg.Domain.Core.Dispatcher
{
    internal class DomainEventHandlersCollection : IDomainEventHandlersCollection
    {
        private readonly Assembly[] _assembliesToScan;

        // Key is the domain event type, the values are the handler types
        private Dictionary<Type, Type[]> _handlers = new Dictionary<Type, Type[]>();

        public DomainEventHandlersCollection(Assembly[] assembliesToScan)
        {
            _assembliesToScan = assembliesToScan;
        }
        
        private Type[] GetHandlerTypeOrThrow(Type entityType)
        {
            if (_handlers.TryGetValue(entityType, out var handlerTypes))
            {
                return handlerTypes;
            }

            return new Type[]{};

        }

        /// <summary>
        /// Scans the provided assemblies for implementations of IEntityStrategy, and stores them internall so they are
        /// available for later use. Call this method once, when your application starts. This class should be available as a singleton
        /// </summary>
        /// <param name="sp">The service provider is necessary because the types for which an IEntityStrategy applies are defined as a property
        /// (see Types array), to read that property this library must create an instance of that implementation</param>
        /// <param name="assembliesToScan"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ScanForHandlers()
        {
            if (_handlers.Any())
            {
                return;
            }
            
            var handlerInterfaceType = typeof(IDomainEventHandler<>);
            var handlersDict = new Dictionary<Type, List<Type>>();

            foreach (var assembly in _assembliesToScan)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var interfaces = type.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

                    foreach (var iface in interfaces)
                    {
                        var eventType = iface.GetGenericArguments()[0];
                        if (!handlersDict.TryGetValue(eventType, out var list))
                        {
                            list = new List<Type>();
                            handlersDict[eventType] = list;
                        }
                        list.Add(type);
                    }
                }
            }

            // Convert List<Type> to Type[] for your _handlers field
            _handlers = handlersDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
            

        }

        /// <summary>
        /// Given a scope, this method creates an instance of the IEntityStrategy or throws an exception if the entityType does not
        /// correspond to any of the IEntityStrategy found by the ScanForStrategies method.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object[] CreateHandlers(IEntityFactoryAdapter factory, IDomainEvent domainEvent)
        {
            var entityType = domainEvent.GetType();
            if (_handlers.TryGetValue(entityType, out var strategyTypes))
            {
                return strategyTypes.Select(t =>
                    factory.Create(t)).Where(x => x != null).Select(x=> x!).ToArray();
            }

            return new object[]{};
        }
    }
}