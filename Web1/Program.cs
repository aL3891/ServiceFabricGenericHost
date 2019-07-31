using ServiceFabricGenericHost;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Web1
{
    internal static class Program
    {
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabricHost()
                .RegisterStatelessService<Web1>("Web1Type")
                .Build()
                .Run();
        }
    }
}
