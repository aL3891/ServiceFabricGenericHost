using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using ServiceFabricGenericHost;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal static class Program
    {
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabric()
                .RegisterServicefabricActor<Actor1, ActorService>()
                .Build()
                .Run();
        }
    }
}
