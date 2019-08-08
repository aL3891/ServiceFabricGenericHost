using System;

namespace ServiceFabricGenericHost
{
    internal class ServicefabricActorDescription
    {
        public Type ServiceType { get; set; }
        public Type ActorType { get; set; }
    }
}
