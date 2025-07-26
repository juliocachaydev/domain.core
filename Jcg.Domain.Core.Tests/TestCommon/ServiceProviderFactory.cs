using Microsoft.Extensions.DependencyInjection;

namespace Jcg.Domain.Core.Tests.TestCommon;

public static class ServiceProviderFactory
{
    public static IServiceProvider Create()
    {
        var serviceCollection = new ServiceCollection();

        var serviceProvider = serviceCollection
            .BuildServiceProvider();

        return serviceProvider;
    }

    public static IServiceProvider Create(Action<IServiceCollection> configureServices)
    {
        var serviceCollection = new ServiceCollection();

        configureServices(serviceCollection);

        var serviceProvider = serviceCollection
            .BuildServiceProvider();

        return serviceProvider;
    }
}