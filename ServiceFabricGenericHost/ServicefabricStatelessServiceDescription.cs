using System;

namespace ServiceFabricGenericHost
{
    internal class ServicefabricStatelessServiceDescription
    {
        public string Name { get; set; }
        public Type ServiceType { get; set; }
    }
}
