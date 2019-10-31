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
        public ServiceFabricRegistrationService( IServiceProvider services )
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        public async Task StartAsync( CancellationToken cancellationToken )
        {

            foreach( var sd in Services.GetService<IEnumerable<ServicefabricServiceDescription>>() )
            {
                var factory = Services.GetRequiredService<GenericHostServiceFactory>();
                await factory.Register( sd, cancellationToken );
            }

            foreach( var sd in Services.GetService<IEnumerable<ServicefabricActorDescription>>() )
            {
                var factory = Services.GetRequiredService<GenericHostActorServiceFactory>();
                await factory.Register( sd, cancellationToken );
            }
        }

        public async Task StopAsync( CancellationToken cancellationToken )
        {

        }
    }
}
