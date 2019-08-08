using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabricGenericHost
{
    internal class ServiceFabricServiceRuntime : IServiceFabricRuntime
    {
        public FabricRuntime Runtime { get; }

        public ServiceFabricServiceRuntime(FabricRuntime runtime)
        {
            Runtime = runtime;
        }

        public Task RegisterStatelessServiceAsync(string serviceTypeName, Func<StatelessServiceContext, StatelessService> serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return ServiceRuntime.RegisterServiceAsync(serviceTypeName, serviceFactory, timeout, cancellationToken);
        }

        public Task RegisterStatefulServiceAsync(string serviceTypeName, Func<StatefulServiceContext, StatefulServiceBase> serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return ServiceRuntime.RegisterServiceAsync(serviceTypeName, serviceFactory, timeout, cancellationToken);
        }

        public Task RegisterStatefulServiceAsync(string serviceTypeName, IStatefulServiceFactory serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return Runtime.RegisterStatefulServiceFactoryAsync(serviceTypeName, serviceFactory, timeout, cancellationToken);
        }

        public Task RegisterStatelessServiceAsync(string serviceTypeName, IStatelessServiceFactory serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return Runtime.RegisterStatelessServiceFactoryAsync(serviceTypeName, serviceFactory, timeout, cancellationToken);
        }

        public Task RegisterActorServiceAsync(Type actorType, Func<StatefulServiceContext, ActorTypeInformation, ActorService> actorServiceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return (Task)typeof(ActorRuntime).GetMethods().First(m => m.GetParameters().Length == 3).MakeGenericMethod(actorType).Invoke(null, System.Reflection.BindingFlags.Static, null, new object[] { actorServiceFactory, timeout, cancellationToken }, null);
        }
    }
}
