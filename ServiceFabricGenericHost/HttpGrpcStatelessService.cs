using System.Fabric;
using System.Fabric.Description;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace ServiceFabricGenericHost
{
    public class HttpGrpcStatelessService<TStartup> : AspnetStatelessService<TStartup> where TStartup : class
    {
        public HttpGrpcStatelessService( StatelessServiceContext serviceContext ) : base( serviceContext ) { }

        public override IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) =>
            webhost.UseKestrel( options => options.Listen( IPAddress.Any, ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } ) );
    }
}
