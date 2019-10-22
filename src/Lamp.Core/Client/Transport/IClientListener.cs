using Lamp.Core.Protocol.Communication;
using System;
using System.Threading.Tasks;

namespace Lamp.Core.Client.Transport
{
    public interface IClientListener
    {
        /// <summary>
        /// 收到消息事件
        /// </summary>
        event Func<IClientSender, TransportMsg, Task> OnReceived;

        /// <summary>
        ///  返回给客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Received(IClientSender sender, TransportMsg message);
    }
}
