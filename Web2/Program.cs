using ServiceFabricGenericHost;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Web2
{
    internal static class Program
    {
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabricHost()
                .RegisterStatefulService<Web2>("Web2Type")
                .Build()
                .Run();

        }
    }
}
