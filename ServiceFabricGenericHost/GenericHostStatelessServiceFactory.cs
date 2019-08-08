using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceFabricGenericHost
{
    public class GenericHostStatelessServiceFactory : IStatelessServiceFactory
    {
        public FabricRuntime Runtime { get; }
        public IServiceProvider ServiceProvider { get; }
        public NodeContext NodeContext { get; }
        public CodePackageActivationContext ActivationContext { get; }
        public Type ServiceType { get; set; }

        public GenericHostStatelessServiceFactory(FabricRuntime fabricRuntime, IServiceProvider serviceProvider)
        {
            Runtime = fabricRuntime;
            ServiceProvider = serviceProvider;
            NodeContext = FabricRuntime.GetNodeContext();
            ActivationContext = FabricRuntime.GetActivationContext();
        }

        public IStatelessServiceInstance CreateInstance(string serviceTypeName, Uri serviceName, byte[] initializationData, Guid partitionId, long instanceId)
        {
            var context = new StatefulServiceContext(NodeContext, ActivationContext, serviceTypeName, serviceName, initializationData, partitionId, instanceId);
            var scope = ServiceProvider.CreateScope();
            scope.ServiceProvider.GetService<ValueHolder<StatefulServiceContext>>().Value = context;
            var service = (IStatelessServiceInstance)scope.ServiceProvider.GetRequiredService(ServiceType);
            return service;
        }
    }
}
