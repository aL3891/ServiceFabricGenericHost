using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabricGenericHost;

namespace RawStateful
{
    internal static class Program
    {
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabric()
                .RegisterServicefabricService<RawStateful>("RawStatefulType")
                .Build()
                .Run();
        }
    }
}
