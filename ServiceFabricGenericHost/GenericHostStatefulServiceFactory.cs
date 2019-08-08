using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceFabricGenericHost
{
    public class GenericHostStatefulServiceFactory : IStatefulServiceFactory
    {
        public FabricRuntime Runtime { get; }
        public IServiceProvider ServiceProvider { get; }
        public NodeContext NodeContext { get; }
        public CodePackageActivationContext ActivationContext { get; }
        public Type ServiceType { get; set; }

        public GenericHostStatefulServiceFactory(FabricRuntime fabricRuntime, IServiceProvider serviceProvider)
        {
            Runtime = fabricRuntime;
            ServiceProvider = serviceProvider;
            NodeContext = FabricRuntime.GetNodeContext();
            ActivationContext = FabricRuntime.GetActivationContext();
        }

        public IStatefulServiceReplica CreateReplica(string serviceTypeName, Uri serviceName, byte[] initializationData, Guid partitionId, long replicaId)
        {
            var context = new StatefulServiceContext(NodeContext, ActivationContext, serviceTypeName, serviceName, initializationData, partitionId, replicaId);
            var scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetService<ValueHolder<StatefulServiceContext>>().Value = context;
            var service = (IStatefulServiceReplica)scope.ServiceProvider.GetRequiredService(ServiceType);
            return service;
        }
    }
}
