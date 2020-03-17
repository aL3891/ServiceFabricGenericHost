using System.Fabric;
using System.Fabric.Description;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace ServiceFabricGenericHost
{
    public class HttpGrpcStatefulService<TStartup> : AspnetStatefulService<TStartup> where TStartup : class
    {
        public HttpGrpcStatefulService( StatefulServiceContext serviceContext ) : base( serviceContext ) { }

        public override IWebHostBuilder Configure( IWebHostBuilder webhost, EndpointResourceDescription ep ) =>
            webhost.UseKestrel( options => options.Listen( IPAddress.Parse("0.0.0.0"), ep.Port, listenoptions => { listenoptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; } ) );
    }
}
