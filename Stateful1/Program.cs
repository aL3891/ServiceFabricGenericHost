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
                .UseServicefabric()
                .RegisterServicefabricService<Stateful1>("Stateful1Type")
                .Build()
                .Run();
        }
    }
}
