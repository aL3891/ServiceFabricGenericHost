using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace RawStateless
{
    internal sealed class RawStateless : IStatelessServiceInstance
    {
        public RawStateless(StatelessServiceContext context)
        { }

        public void Initialize(StatelessServiceInitializationParameters initializationParameters)
        {
        }

        public async Task<string> OpenAsync(IStatelessServicePartition partition, CancellationToken cancellationToken)
        {
            return @"{""Endpoints"":{""Listener1"":""Endpoint1"",""Listener2"":""Endpoint2"" }}";
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Abort()
        {

        }
    }
}
