using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ServiceFabricGenericHost
{
    public static class HostExtensions
    {
        public static IHostBuilder RegisterStatelessService<TService>(this IHostBuilder hostBuilder, string name) where TService : class
        {
            hostBuilder.ConfigureServices((builderContext, services) =>
            {
                services.AddScoped<TService, TService>();
                services.AddSingleton(new ServicefabricStatelessServiceDescription { Name = name, ServiceType = typeof(TService) });
            });
            return hostBuilder;
        }

        public static IHostBuilder RegisterStatefulService<TService>(this IHostBuilder hostBuilder, string name) where TService : class
        {
            hostBuilder.ConfigureServices((builderContext, services) =>
            {
                services.AddScoped<TService, TService>();
                services.AddSingleton(new ServicefabricStatefulServiceDescription { Name = name, ServiceType = typeof(TService) });
            });
            return hostBuilder;
        }

        public static IHostBuilder RegisterActorService<TActor, TService>(this IHostBuilder hostBuilder) where TService : class
        {
            hostBuilder.ConfigureServices((builderContext, services) =>
            {
                services.AddScoped<TService, TService>();
                services.AddSingleton(new ServicefabricActorServiceDescription { ServiceType = typeof(TService), ActorType = typeof(TActor) });
            });
            return hostBuilder;
        }

        public static IHostBuilder UseServicefabricHost(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((builderContext, services) =>
            {
                services.AddHostedService<ServiceFabricRegistrationService>();

                services.AddSingleton<IServiceFabricRuntime, ServiceFabricServiceRuntime>();
                services.AddScoped<ValueHolder<StatelessServiceContext>, ValueHolder<StatelessServiceContext>>();
                services.AddScoped<ValueHolder<StatefulServiceContext>, ValueHolder<StatefulServiceContext>>();
                services.AddScoped<ValueHolder<ActorTypeInformation>, ValueHolder<ActorTypeInformation>>();

                services.AddScoped<ValueHolder<IActorStateProvider>, ValueHolder<IActorStateProvider>>();
                services.AddScoped<ValueHolder<ActorServiceSettings>, ValueHolder<ActorServiceSettings>>();
                services.AddScoped<ValueHolder<ActorService>, ValueHolder<ActorService>>();
                services.AddScoped<ValueHolder<ActorId>, ValueHolder<ActorId>>();
                services.AddScoped<ValueHolder<Func<ActorService, ActorId, ActorBase>>>();


                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<StatelessServiceContext>>().Value);
                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<StatefulServiceContext>>().Value);
                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<ActorTypeInformation>>().Value);

                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<IActorStateProvider>>().Value);
                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<ActorServiceSettings>>().Value);
                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<ActorService>>().Value);
                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<ActorId>>().Value);
                services.AddTransient(sp => sp.GetRequiredService<ValueHolder<Func<ActorService, ActorId, ActorBase>>>().Value);

            });

            return hostBuilder;
        }
    }
}
