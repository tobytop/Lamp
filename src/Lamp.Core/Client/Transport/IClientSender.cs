using Lamp.Core.Protocol.Communication;
using System.Threading.Tasks;

namespace Lamp.Core.Client.Transport
{
    public interface IClientSender
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task SendAsync(TransportMsg msg);
    }
}
