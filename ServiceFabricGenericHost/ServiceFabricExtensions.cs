using System.Fabric;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace ServiceFabricGenericHost
{
    public static class ServiceFabricExtensions
    {
        public static ServiceReplicaListener HttpAspnetListener<TStartup>( this StatefulServiceContext serviceContext, string endpointName = "GrpcEndpoint" ) where TStartup : class
        {
            return HostExtensions.ToReplicaListener( ep => Host.CreateDefaultBuilder()
                  .ConfigureWebHostDefaults( webhost => webhost
                  .UseKestrel( options =>
                  {
                      options.Listen( IPAddress.Any, ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } );
                      options.AllowSynchronousIO = true;
                  } )
                  .UseStartup<TStartup>()
                  .UseEndPoints( ep ) ), serviceContext.NodeContext, serviceContext.CodePackageActivationContext.GetEndpoint( endpointName ) );
        }

        public static ServiceInstanceListener HttpAspnetListener<TStartup>( this StatelessServiceContext serviceContext, string endpointName = "GrpcEndpoint" ) where TStartup : class
        {
            return HostExtensions.ToInstanceListener( ep =>
            Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults( webhost => webhost
            .UseKestrel( options =>
            {
                options.Listen( IPAddress.Any, ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } );
                options.AllowSynchronousIO = true;
            } )
            .UseStartup<TStartup>()
            .UseEndPoints( ep ) ), serviceContext.NodeContext, serviceContext.CodePackageActivationContext.GetEndpoint( endpointName ) );
        }
    }
}
