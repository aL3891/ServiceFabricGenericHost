using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabricGenericHost
{
    public class GenericHostActorServiceFactory
    {
        public IServiceFabricRuntime Runtime { get; }
        public IServiceProvider ServiceProvider { get; }
        public Type ServiceType { get; set; }
        public Type ActorType { get; set; }
        public IServiceScope scope { get; private set; }
        public ActorService service { get; private set; }

        public GenericHostActorServiceFactory( IServiceFabricRuntime runtime, IServiceProvider serviceProvider )
        {
            Runtime = runtime;
            ServiceProvider = serviceProvider;
        }

        public ActorService Factory( StatefulServiceContext context, ActorTypeInformation actorTypeInfo )
        {
            scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetService<ValueHolder<StatefulServiceContext>>().Value = context;
            scope.ServiceProvider.GetService<ValueHolder<ActorTypeInformation>>().Value = actorTypeInfo;
            scope.ServiceProvider.GetService<ValueHolder<IActorStateProvider>>().Value = null;
            scope.ServiceProvider.GetService<ValueHolder<ActorServiceSettings>>().Value = null;
            scope.ServiceProvider.GetService<ValueHolder<Func<ActorService, ActorId, ActorBase>>>().Value = ActorFactory;
            service = (ActorService)scope.ServiceProvider.GetRequiredService( ServiceType );
            return service;
        }

        public ActorBase ActorFactory( ActorService actorService, ActorId actorId )
        {
            var actorScope = scope.ServiceProvider.CreateScope();
            actorScope.ServiceProvider.GetService<ValueHolder<ActorService>>().Value = actorService;
            actorScope.ServiceProvider.GetService<ValueHolder<ActorId>>().Value = actorId;
            return (ActorBase)actorScope.ServiceProvider.GetRequiredService( ActorType );
        }

        internal async Task Register( ServicefabricActorDescription sd, CancellationToken cancellationToken )
        {
            ServiceType = sd.ServiceType;
            ActorType = sd.ActorType;
            await Runtime.RegisterActorServiceAsync( sd.ActorType, Factory, default, cancellationToken );
        }
    }
}
