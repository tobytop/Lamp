using System.Collections.Generic;

namespace Lamp.Core.Protocol.Server
{
    public class ServerDesc
    {
        public ServerAddress ServerAddress { get; set; }

        public List<ServiceDesc> ServiceDescriptor { get; set; }
    }
}
