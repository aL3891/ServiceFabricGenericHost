﻿using System;
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
using Microsoft.AspNetCore.Hosting;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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


        public static ServiceInstanceListener ToInstanceListener( Func<EndpointResourceDescription, IHostBuilder> builderFactory, NodeContext nodeContext, EndpointResourceDescription endpoint )
        {
            return new ServiceInstanceListener( serviceContext => new GenericHostCommunicationListener( endpoint, nodeContext, builderFactory, serviceContext ), endpoint.Name );
        }

        public static IEnumerable<ServiceInstanceListener> ToInstanceListeners( Func<EndpointResourceDescription, IHostBuilder> builderFactory, NodeContext nodeContext, IEnumerable<EndpointResourceDescription> endpoint )
        {
            bool first = true;
            foreach( var ep in endpoint )
            {
                if( first )
                {
                    yield return new ServiceInstanceListener( serviceContext => new GenericHostCommunicationListener( ep, nodeContext, builderFactory, serviceContext ), ep.Name );
                    first = false;
                }
                else
                    yield return new ServiceInstanceListener( serviceContext => new FakeCommunicationListener( $"{ep.Protocol}://{nodeContext.IPAddressOrFQDN}:{ep.Port}" ), ep.Name );
            }
        }

        public static ServiceReplicaListener ToReplicaListener( Func<EndpointResourceDescription, IHostBuilder> builderFactory, NodeContext nodeContext, EndpointResourceDescription endpoint )
        {
            return new ServiceReplicaListener( serviceContext => new GenericHostCommunicationListener( endpoint, nodeContext, builderFactory, serviceContext ), endpoint.Name );
        }

        public static IEnumerable<ServiceReplicaListener> ToReplicaListeners( Func<EndpointResourceDescription, IHostBuilder> builderFactory, NodeContext nodeContext, IEnumerable<EndpointResourceDescription> endpoint )
        {
            bool first = true;
            foreach( var ep in endpoint )
            {
                if( first )
                {
                    yield return new ServiceReplicaListener( serviceContext => new GenericHostCommunicationListener( ep, nodeContext, builderFactory, serviceContext ), ep.Name );
                    first = false;
                }
                else
                    yield return new ServiceReplicaListener( serviceContext => new FakeCommunicationListener( $"{ep.Protocol}://{nodeContext.IPAddressOrFQDN}:{ep.Port}" ), ep.Name );
            }
        }
    }
}
