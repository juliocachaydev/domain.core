using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Jcg.Domain.Core.Dispatcher;
using Jcg.Domain.Core.LibrarySupport;
using Jcg.Domain.Core.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Jcg.Domain.Core.Microsoft.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddRepository(
            this IServiceCollection services,
            Func<IServiceProvider, IDatabaseAdapter> dbAdapterFactory,
            Assembly firstAssemblyToScan,
            params Assembly[] additionalAssemgbliesToScan)
        {
            var assembliesToScan = additionalAssemgbliesToScan.Append(firstAssemblyToScan)
                .ToArray();
            
            services.AddScoped(dbAdapterFactory);
            services.AddSingleton<IDomainEventHandlersCollection>(_ => 
                DomainEventHandlersCollectionFactory.CreateDomainEventHandlersCollection(assembliesToScan));
            
            services.AddScoped<IDomainEventDispatcher>(sp => DomainEventDispatcherFactory.Create(
                sp.GetRequiredService<IDomainEventHandlersCollection>(),
                sp.GetRequiredService<IEntityFactoryAdapter>()));
            
            services.AddSingleton<RepositoryStrategyCollection>(_ => new RepositoryStrategyCollection(assembliesToScan));
            services.AddScoped<IEntityFactoryAdapter, EntityFactoryAdapter>();

            services.AddScoped<IRepository>(sp =>
                RepositoryFactory.Create(sp.GetRequiredService<IDatabaseAdapter>(),
                    sp.GetRequiredService<RepositoryStrategyCollection>(),
                    sp.GetRequiredService<IDomainEventDispatcher>(),
                    sp.GetRequiredService<IEntityFactoryAdapter>())
            );
        }

        internal class EntityFactoryAdapter : IEntityFactoryAdapter
        {
            private readonly IServiceProvider _sp;

            public EntityFactoryAdapter(IServiceProvider sp)
            {
                _sp = sp;
            }

            public object? Create(Type type)
            {
                return ActivatorUtilities.CreateInstance(_sp, type);
            }
        }
}
}