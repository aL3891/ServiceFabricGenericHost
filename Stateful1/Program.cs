using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ServiceFabricGenericHost;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Stateful1
{
    internal static class Program
    {
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabricHost()
                .RegisterStatefulService<Stateful1>("Stateful1Type")
                .Build()
                .Run();
        }
    }
}
