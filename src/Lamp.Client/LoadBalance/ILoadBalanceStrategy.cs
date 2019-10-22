using Lamp.Core.Protocol.Server;
using System.Collections.Generic;

namespace Lamp.Client.LoadBalance
{
    public interface ILoadBalanceStrategy
    {
        ServerAddress DoLoadBalance(List<ServerAddress> addresses);
    }
}
