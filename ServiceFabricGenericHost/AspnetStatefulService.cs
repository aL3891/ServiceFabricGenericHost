using System.Fabric;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.AspNetCore.Hosting;

namespace ServiceFabricGenericHost
{
    public class AspnetStatefulService<TStartup> : Microsoft.ServiceFabric.Services.Runtime.StatefulService where TStartup : class
    {
        public AspnetStatefulService( StatefulServiceContext serviceContext ) : base( serviceContext ) { }

        public virtual IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) => webhost.UseKestrel();

        public virtual IHostBuilder Configure( IHostBuilder hostBuilder ) => hostBuilder;

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var l = HostExtensions.ToReplicaListener( ep =>
            Configure( Host.CreateDefaultBuilder().ConfigureWebHostDefaults( webhost => Configure( webhost, ep ).UseStartup<TStartup>().UseEndPoints( ep ) ) ), Context.NodeContext, Context.CodePackageActivationContext.GetEndpoint( "GrpcEndpoint" ) );
            return new ServiceReplicaListener[] { l };
        }
    }
}
