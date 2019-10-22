using System.Collections.Generic;

namespace Lamp.Core.Protocol.Communication
{
    /// <summary>
    /// RPC服务自定义头部
    /// </summary>
    public class Payload
    {
        public IDictionary<string, object> Items { get; set; }
    }
}
