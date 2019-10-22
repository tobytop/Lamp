using Lamp.Core.Client.Transport;
using Lamp.Core.Protocol.Communication;
using System;
using System.Threading.Tasks;

namespace Lamp.Client.Transport
{
    public class RpcClientListener : IClientListener
    {
        public event Func<IClientSender, TransportMsg, Task> OnReceived;

        public async Task Received(IClientSender sender, TransportMsg message)
        {
            if (OnReceived == null)
            {
                return;
            }

            await OnReceived(sender, message);
        }
    }
}
