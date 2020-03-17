using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabricGenericHost
{
    public class FakeCommunicationListener : ICommunicationListener
    {
        public string Url { get; }

        public FakeCommunicationListener( string url )
        {
            Url = url;
        }

        public async Task<string> OpenAsync( CancellationToken cancellationToken )
        {
            return Url;
        }

        public async Task CloseAsync( CancellationToken cancellationToken )
        {

        }

        public void Abort()
        {

        }
    }
}
