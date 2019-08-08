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
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabric()
                .RegisterServicefabricService<Stateless1>("Stateless1Type")
                .Build()
                .Run();
        }
    }
}
