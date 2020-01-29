using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ServiceFabricGenericHost
{
    public class GenericHostCommunicationListener : ICommunicationListener
    {
        public EndpointResourceDescription Endpoint { get; }
        public NodeContext NodeContext { get; }
        public Func<EndpointResourceDescription,IHostBuilder> HostBuilderFactory { get; }
        public ServiceContext ServiceContext { get; }
        public IHost Host { get; private set; }

        public async Task<string> OpenAsync( CancellationToken cancellationToken )
        {
            var builder = HostBuilderFactory(Endpoint).ConfigureServices( services => services.AddSingleton(ServiceContext.GetType(), ServiceContext ) );
            Host = await builder.StartAsync( cancellationToken );
            return $"{Endpoint.Protocol}://{NodeContext.IPAddressOrFQDN}:{Endpoint.Port}";
        }

        public async Task CloseAsync( CancellationToken cancellationToken )
        {
            await Host.StopAsync( cancellationToken );
        }

        public void Abort()
        {
            Host?.StopAsync();
        }

        public GenericHostCommunicationListener( EndpointResourceDescription endpoint, NodeContext nodeContext, Func<EndpointResourceDescription,IHostBuilder> builder, ServiceContext serviceContext )
        {
            Endpoint = endpoint;
            NodeContext = nodeContext;
            HostBuilderFactory = builder;
            ServiceContext = serviceContext;
        }
    }
}
