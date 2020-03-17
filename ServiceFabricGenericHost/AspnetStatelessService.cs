using System.Fabric;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.AspNetCore.Hosting;

namespace ServiceFabricGenericHost
{
    public class AspnetStatelessService<TStartup> : Microsoft.ServiceFabric.Services.Runtime.StatelessService where TStartup : class
    {
        public AspnetStatelessService( StatelessServiceContext serviceContext ) : base( serviceContext ) { }

        public virtual IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) => webhost.UseKestrel();

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var l = HostExtensions.ToInstanceListener( ep => Host.CreateDefaultBuilder()
                 .ConfigureWebHostDefaults( webhost => Configure( webhost, ep ).UseStartup<TStartup>().UseEndPoints( ep ) ), Context.NodeContext, Context.CodePackageActivationContext.GetEndpoint( "GrpcEndpoint" ) );

            return new ServiceInstanceListener[] { l };
        }
    }
}
