using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabricGenericHost
{
    public interface IServiceFabricRuntime
    {
        Task RegisterStatelessServiceAsync(string serviceTypeName, Func<StatelessServiceContext, StatelessService> serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task RegisterStatefulServiceAsync(string serviceTypeName, Func<StatefulServiceContext, StatefulServiceBase> serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task RegisterActorServiceAsync(Type actorType, Func<StatefulServiceContext, ActorTypeInformation, ActorService> actorServiceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default);
    }
}
