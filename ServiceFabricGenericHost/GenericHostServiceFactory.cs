using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabricGenericHost
{
    public class GenericHostServiceFactory : IStatefulServiceFactory, IStatelessServiceFactory
    {
        public IServiceFabricRuntime Runtime { get; }
        public IServiceProvider ServiceProvider { get; }
        public Type ServiceType { get; set; }

        public GenericHostServiceFactory( IServiceFabricRuntime runtime, IServiceProvider serviceProvider )
        {
            Runtime = runtime;
            ServiceProvider = serviceProvider;
        }

        internal async Task Register( ServicefabricServiceDescription sd, CancellationToken cancellationToken )
        {
            ServiceType = sd.ServiceType;
            await (ServiceType switch
            {
                var t when typeof( IStatelessServiceInstance ).IsAssignableFrom( t ) => Runtime.RegisterStatelessServiceAsync( sd.Name, this, default, cancellationToken ),
                var t when typeof( IStatefulServiceReplica ).IsAssignableFrom( t ) => Runtime.RegisterStatefulServiceAsync( sd.Name, this, default, cancellationToken ),
                var t when typeof( StatelessService ).IsAssignableFrom( t ) => Runtime.RegisterStatelessServiceAsync( sd.Name, this.Factory, default, cancellationToken ),
                var t when typeof( StatefulService ).IsAssignableFrom( t ) => Runtime.RegisterStatefulServiceAsync( sd.Name, this.Factory, default, cancellationToken )
            });
        }

        public IStatefulServiceReplica CreateReplica( string serviceTypeName, Uri serviceName, byte[] initializationData, Guid partitionId, long replicaId )
        {
            var context = new StatefulServiceContext( Runtime.GetNodeContext(), Runtime.GetActivationContext(), serviceTypeName, serviceName, initializationData, partitionId, replicaId );
            var scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetService<ValueHolder<StatefulServiceContext>>().Value = context;
            var service = (IStatefulServiceReplica)scope.ServiceProvider.GetRequiredService( ServiceType );
            return service;
        }

        public StatefulService Factory( StatefulServiceContext context )
        {
            var scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetService<ValueHolder<StatefulServiceContext>>().Value = context;
            var service = (StatefulService)scope.ServiceProvider.GetRequiredService( ServiceType );
            return service;
        }

        public IStatelessServiceInstance CreateInstance( string serviceTypeName, Uri serviceName, byte[] initializationData, Guid partitionId, long instanceId )
        {
            var context = new StatelessServiceContext( Runtime.GetNodeContext(), Runtime.GetActivationContext(), serviceTypeName, serviceName, initializationData, partitionId, instanceId );
            var scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetService<ValueHolder<StatelessServiceContext>>().Value = context;
            var service = (IStatelessServiceInstance)scope.ServiceProvider.GetRequiredService( ServiceType );
            return service;
        }

        public StatelessService Factory( StatelessServiceContext context )
        {
            var scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetService<ValueHolder<StatelessServiceContext>>().Value = context;
            var service = (StatelessService)scope.ServiceProvider.GetRequiredService( ServiceType );
            return service;
        }
    }
}
