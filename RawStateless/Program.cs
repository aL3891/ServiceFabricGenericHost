using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabricGenericHost;

namespace RawStateless
{
    internal static class Program
    {
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .UseServicefabric()
                .RegisterServicefabricService<RawStateless>("RawStatelessType")
                .Build()
                .Run();
        }
    }
}
