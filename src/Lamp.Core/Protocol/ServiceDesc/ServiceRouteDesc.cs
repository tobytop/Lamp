using Lamp.Core.Protocol.Server;
using System.Collections.Generic;

namespace Lamp.Core.Protocol
{
    public class ServiceRouteDesc
    {
        public IEnumerable<AddressDesc> AddressDescriptors { get; set; }

        public ServiceDesc ServiceDescriptor { get; set; }
    }
}
