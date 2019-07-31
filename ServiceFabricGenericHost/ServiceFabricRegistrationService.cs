using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabricGenericHost
{
    internal class ServiceFabricRegistrationService : IHostedService
    {
        public ServiceFabricRegistrationService(IServiceProvider services, ILogger<ServiceFabricRegistrationService> logger)
        {
            Services = services;
            Logger = logger;
        }
        public IServiceProvider Services { get; }
        public ILogger<ServiceFabricRegistrationService> Logger { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var runtime = Services.GetRequiredService<IServiceFabricRuntime>();

            foreach (var sd in Services.GetService<IEnumerable<ServicefabricStatelessServiceDescription>>())
            {
                await runtime.RegisterStatelessServiceAsync(sd.Name, context =>
                {
                    var scope = Services.CreateScope();
                    scope.ServiceProvider.GetService<ValueHolder<StatelessServiceContext>>().Value = context;
                    var service = scope.ServiceProvider.GetRequiredService(sd.ServiceType);
                    return (StatelessService)service;
                }, default, cancellationToken);
            }

            foreach (var sd in Services.GetService<IEnumerable<ServicefabricStatefulServiceDescription>>())
            {
                await runtime.RegisterStatefulServiceAsync(sd.Name, context =>
                {
                    var scope = Services.CreateScope();
                    scope.ServiceProvider.GetService<ValueHolder<StatefulServiceContext>>().Value = context;
                    var service = scope.ServiceProvider.GetRequiredService(sd.ServiceType);
                    return (StatefulService)service;
                }, default, cancellationToken);
            }

            foreach (var sd in Services.GetService<IEnumerable<ServicefabricActorServiceDescription>>())
            {
                await runtime.RegisterActorServiceAsync(sd.ActorType, (context, actorTypeInfo) =>
                {
                    var scope = Services.CreateScope();
                    scope.ServiceProvider.GetService<ValueHolder<StatefulServiceContext>>().Value = context;
                    scope.ServiceProvider.GetService<ValueHolder<ActorTypeInformation>>().Value = actorTypeInfo;
                    scope.ServiceProvider.GetService<ValueHolder<IActorStateProvider>>().Value = null;
                    scope.ServiceProvider.GetService<ValueHolder<ActorServiceSettings>>().Value = null;

                    scope.ServiceProvider.GetService<ValueHolder<Func<ActorService, ActorId, ActorBase>>>().Value = (service, id) =>
                    {
                        var actorScope = scope.ServiceProvider.CreateScope();
                        actorScope.ServiceProvider.GetService<ValueHolder<ActorService>>().Value = service;
                        actorScope.ServiceProvider.GetService<ValueHolder<ActorId>>().Value = id;
                        return (ActorBase)actorScope.ServiceProvider.GetRequiredService(sd.ActorType);
                    };

                    var service = scope.ServiceProvider.GetRequiredService(sd.ServiceType);
                    return (ActorService)service;
                }, default, cancellationToken);
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {

        }
    }
}
