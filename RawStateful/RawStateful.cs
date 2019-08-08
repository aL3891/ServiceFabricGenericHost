using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace RawStateful
{
    internal sealed class RawStateful : IStatefulServiceReplica
    {
        public IReplicator Replicator { get; private set; }
        public IStateProviderReplica StateProviderReplica { get; }
        public IReliableStateManager ReliableStateManager { get; }

        public RawStateful(IStateProviderReplica stateProviderReplica, IReliableStateManager reliableStateManager)
        {
            StateProviderReplica = stateProviderReplica;
            ReliableStateManager = reliableStateManager;
            StateProviderReplica.OnDataLossAsync = OnDataLossAsync;
        }

        private async Task<bool> OnDataLossAsync(CancellationToken cancellationToken)
        {
            return true;
        }


        public void Initialize(StatefulServiceInitializationParameters initializationParameters)
        {
            StateProviderReplica.Initialize(initializationParameters);
        }

        public async Task<IReplicator> OpenAsync(ReplicaOpenMode openMode, IStatefulServicePartition partition, CancellationToken cancellationToken)
        {
            Replicator = await StateProviderReplica.OpenAsync(openMode, partition, cancellationToken);
            return Replicator;
        }

        public async Task<string> ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            return @"{""Endpoints"":{""Listener1"":""Endpoint1"" }}".Replace("Endpoint1", StateProviderReplica.GetHashCode().ToString());
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await StateProviderReplica.CloseAsync(cancellationToken);
        }

        public void Abort()
        {
            StateProviderReplica.Abort();
        }
    }
}
