using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using ServiceFabricGenericHost;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Stateless1
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabricHost()
                .RegisterStatelessService<Stateless1>("Stateless1Type")
                .Build()
                .Run();
        }
    }
}
