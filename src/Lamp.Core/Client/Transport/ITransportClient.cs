using Lamp.Core.Protocol.Communication;
using System.Threading.Tasks;

namespace Lamp.Core.Client.Transport
{
    public interface ITransportClient
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="invokeMessage"></param>
        /// <returns></returns>
        Task<RemoteCallBackData> SendAsync(RemoteCallData invokeMessage);
    }
}
