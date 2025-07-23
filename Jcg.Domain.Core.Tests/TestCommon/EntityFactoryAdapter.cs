using Jcg.Domain.Core.LibrarySupport;
using Microsoft.Extensions.DependencyInjection;

namespace Jcg.Domain.Core.Tests.TestCommon;

public class EntityFactoryAdapter : IEntityFactoryAdapter
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