using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using System.Collections.Generic;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace ServiceFabricGenericHost
{
    public static class HostExtensions
    {
        public static IHostBuilder RegisterServicefabricService<TService>( this IHostBuilder hostBuilder, string name ) where TService : class
        {
            hostBuilder.ConfigureServices( ( builderContext, services ) =>
             {
                 services.AddScoped<TService, TService>();
                 services.AddSingleton( new ServicefabricServiceDescription { Name = name, ServiceType = typeof( TService ) } );
             } );
            return hostBuilder;
        }

        public static IHostBuilder RegisterServicefabricActor<TActor, TService>( this IHostBuilder hostBuilder ) where TService : class
        {
            hostBuilder.ConfigureServices( ( builderContext, services ) =>
             {
                 services.AddScoped<TService, TService>();
                 services.AddSingleton( new ServicefabricActorDescription { ServiceType = typeof( TService ), ActorType = typeof( TActor ) } );
             } );
            return hostBuilder;
        }

        public static IHostBuilder UseServicefabric( this IHostBuilder hostBuilder )
        {
            hostBuilder.ConfigureServices( ( builderContext, services ) =>
             {
                 services.AddHostedService<ServiceFabricRegistrationService>();

                 services.AddSingleton<IServiceFabricRuntime, ServiceFabricServiceRuntime>();
                 services.AddTransient<GenericHostServiceFactory, GenericHostServiceFactory>();

                 services.AddScoped<ValueHolder<StatelessServiceContext>, ValueHolder<StatelessServiceContext>>();
                 services.AddScoped<ValueHolder<StatefulServiceContext>, ValueHolder<StatefulServiceContext>>();
                 services.AddScoped<ValueHolder<ActorTypeInformation>, ValueHolder<ActorTypeInformation>>();

                 services.AddScoped<ValueHolder<IActorStateProvider>, ValueHolder<IActorStateProvider>>();
                 services.AddScoped<ValueHolder<ActorServiceSettings>, ValueHolder<ActorServiceSettings>>();
                 services.AddScoped<ValueHolder<ActorService>, ValueHolder<ActorService>>();
                 services.AddScoped<ValueHolder<ActorId>, ValueHolder<ActorId>>();
                 services.AddScoped<ValueHolder<Func<ActorService, ActorId, ActorBase>>>();

                 services.AddScoped<ReliableStateManager, ReliableStateManager>();
                 services.AddScoped<IReliableStateManager>( sp => sp.GetRequiredService<ReliableStateManager>() );
                 services.AddScoped<IStateProviderReplica>( sp => sp.GetRequiredService<ReliableStateManager>() );

                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<StatelessServiceContext>>().Value );
                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<StatefulServiceContext>>().Value );
                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<ActorTypeInformation>>().Value );

                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<IActorStateProvider>>().Value );
                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<ActorServiceSettings>>().Value );
                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<ActorService>>().Value );
                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<ActorId>>().Value );
                 services.AddTransient( sp => sp.GetRequiredService<ValueHolder<Func<ActorService, ActorId, ActorBase>>>().Value );

             } );

            return hostBuilder;
        }

        public static IWebHostBuilder UseEndPoints( this IWebHostBuilder builder, IEnumerable<EndpointResourceDescription> endpoints )
        {
            return builder.UseUrls( endpoints.Select( endpoint => $"{endpoint.Protocol}://+:{endpoint.Port}" ).ToArray() );
        }

        public static IWebHostBuilder UseEndPoints( this IWebHostBuilder builder, EndpointResourceDescription endpoint )
        {
            return builder.UseUrls( $"{endpoint.Protocol}://+:{endpoint.Port}" );
        }

        public static ServiceInstanceListener ToInstanceListener( this IHostBuilder builder, NodeContext nodeContext, EndpointResourceDescription endpoint )
        {
            return new ServiceInstanceListener( serviceContext => new GenericHostCommunicationListener( endpoint, nodeContext, builder.ConfigureServices( services => services.AddSingleton( serviceContext ) ) ), endpoint.Name );
        }

        public static IEnumerable<ServiceInstanceListener> ToInstanceListeners( this IHostBuilder builder, NodeContext nodeContext, IEnumerable<EndpointResourceDescription> endpoint )
        {
            bool first = true;
            foreach( var ep in endpoint )
            {
                if( first )
                {
                    yield return new ServiceInstanceListener( serviceContext => new GenericHostCommunicationListener( ep, nodeContext, builder.ConfigureServices( services => services.AddSingleton( serviceContext ) ) ), ep.Name );
                    first = false;
                }
                else
                    yield return new ServiceInstanceListener( serviceContext => new FakeCommunicationListener( $"{ep.Protocol}://{nodeContext.IPAddressOrFQDN}:{ep.Port}" ), ep.Name );
            }
        }

        public static ServiceReplicaListener ToReplicaListener( this IHostBuilder builder, NodeContext nodeContext, EndpointResourceDescription endpoint )
        {
            return new ServiceReplicaListener( serviceContext => new GenericHostCommunicationListener( endpoint, nodeContext, builder.ConfigureServices( services => services.AddSingleton( serviceContext ) ) ), endpoint.Name );
        }

        public static IEnumerable<ServiceReplicaListener> ToReplicaListeners( this IHostBuilder builder, NodeContext nodeContext, IEnumerable<EndpointResourceDescription> endpoint )
        {
            bool first = true;
            foreach( var ep in endpoint )
            {
                if( first )
                {
                    yield return new ServiceReplicaListener( serviceContext => new GenericHostCommunicationListener( ep, nodeContext, builder.ConfigureServices( services => services.AddSingleton( serviceContext ) ) ), ep.Name );
                    first = false;
                }
                else
                    yield return new ServiceReplicaListener( serviceContext => new FakeCommunicationListener( $"{ep.Protocol}://{nodeContext.IPAddressOrFQDN}:{ep.Port}" ), ep.Name );
            }
        }
    }

    public static class ServiceFabricExtensions
    {
        public static ServiceReplicaListener HttpAspnetListener<TStartup>( this StatefulServiceContext serviceContext, string endpointName = "GrpcEndpoint" ) where TStartup : class
        {
            var ep = serviceContext.CodePackageActivationContext.GetEndpoint( endpointName );
            return Host.CreateDefaultBuilder()
                 .ConfigureWebHostDefaults( webhost => webhost
                 .UseKestrel( options =>
                 {
                     options.Listen( IPAddress.Any, ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } );
                     options.AllowSynchronousIO = true;
                 } )
                 .UseStartup<TStartup>()
                 .UseEndPoints( ep ) )
                 .ToReplicaListener( serviceContext.NodeContext, ep );
        }

        public static ServiceInstanceListener HttpAspnetListener<TStartup>( this StatelessServiceContext serviceContext, string endpointName = "GrpcEndpoint" ) where TStartup : class
        {
            var ep = serviceContext.CodePackageActivationContext.GetEndpoint( endpointName );
            return Host.CreateDefaultBuilder()
                 .ConfigureWebHostDefaults( webhost => webhost
                 .UseKestrel( options =>
                 {
                     options.Listen( IPAddress.Any, ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } );
                     options.AllowSynchronousIO = true;
                 } )
                 .UseStartup<TStartup>()
                 .UseEndPoints( ep ) )
                 .ToInstanceListener( serviceContext.NodeContext, ep );
        }
    }


    public class AspnetStatefulService<TStartup> : Microsoft.ServiceFabric.Services.Runtime.StatefulService where TStartup : class
    {
        public AspnetStatefulService( StatefulServiceContext serviceContext ) : base( serviceContext ) { }

        public virtual IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) => webhost.UseKestrel();

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var ep = Context.CodePackageActivationContext.GetEndpoint( "GrpcEndpoint" );
            var l = Host.CreateDefaultBuilder()
                 .ConfigureWebHostDefaults( webhost => Configure( webhost, ep ).UseStartup<TStartup>().UseEndPoints( ep ) )
                 .ToReplicaListener( Context.NodeContext, ep );
            return new ServiceReplicaListener[] { l };
        }
    }

    public class HttpGrpcStatefulService<TStartup> : AspnetStatefulService<TStartup> where TStartup : class
    {
        public HttpGrpcStatefulService( StatefulServiceContext serviceContext ) : base( serviceContext ) { }

        public override IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) =>
            webhost.UseKestrel( options => options.Listen( IPAddress.Any, ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } ) );
    }

    public class AspnetStatelessService<TStartup> : Microsoft.ServiceFabric.Services.Runtime.StatelessService where TStartup : class
    {
        public AspnetStatelessService( StatelessServiceContext serviceContext ) : base( serviceContext ) { }

        public virtual IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) => webhost.UseKestrel();

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var ep = Context.CodePackageActivationContext.GetEndpoint( "GrpcEndpoint" );
            var l = Host.CreateDefaultBuilder()
                 .ConfigureWebHostDefaults( webhost => Configure( webhost, ep ).UseStartup<TStartup>().UseEndPoints( ep ) )
                 .ToInstanceListener( Context.NodeContext, ep );
            return new ServiceInstanceListener[] { l };
        }
    }

    public class HttpGrpcStatelessService<TStartup> : AspnetStatelessService<TStartup> where TStartup : class
    {
        public HttpGrpcStatelessService( StatelessServiceContext serviceContext ) : base( serviceContext ) { }

        public override IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) =>
            webhost.UseKestrel( options => options.Listen( IPAddress.Any, ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } ) );
    }

    public class FakeCommunicationListener : ICommunicationListener
    {
        public string Url { get; }

        public FakeCommunicationListener( string url )
        {
            Url = url;
        }

        public async Task<string> OpenAsync( CancellationToken cancellationToken )
        {
            return Url;
        }

        public async Task CloseAsync( CancellationToken cancellationToken )
        {

        }

        public void Abort()
        {

        }
    }

    public class GenericHostCommunicationListener : ICommunicationListener
    {
        public EndpointResourceDescription Ep { get; }
        public NodeContext NodeContext { get; }
        public IHostBuilder HostBuilder { get; }
        public IHost Host { get; private set; }

        public async Task<string> OpenAsync( CancellationToken cancellationToken )
        {
            Host = await HostBuilder.StartAsync( cancellationToken );
            return $"{Ep.Protocol}://{NodeContext.IPAddressOrFQDN}:{Ep.Port}";
        }

        public async Task CloseAsync( CancellationToken cancellationToken )
        {
            await Host.StopAsync( cancellationToken );
        }

        public void Abort()
        {
            Host?.StopAsync();
        }

        public GenericHostCommunicationListener( EndpointResourceDescription ep, NodeContext nodeContext, IHostBuilder host )
        {
            Ep = ep;
            NodeContext = nodeContext;
            HostBuilder = host;
        }
    }
}
