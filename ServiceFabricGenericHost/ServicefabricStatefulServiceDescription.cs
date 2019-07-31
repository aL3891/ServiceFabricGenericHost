using System;

namespace ServiceFabricGenericHost
{
    internal class ServicefabricStatefulServiceDescription
    {
        public string Name { get; set; }
        public Type ServiceType { get; set; }
    }
}
