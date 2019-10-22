using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Core.Client.RemoteExecutor
{
    public interface IRemoteServiceExecutor
    {
        /// <summary>
        /// 服务器路径执行器
        /// </summary>
        /// <param name="serviceIdOrPath"></param>
        /// <param name="paras"></param>
        /// <param name="token"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        Task<RemoteCallBackData> InvokeAsync(string serviceIdOrPath, IDictionary<string, object> paras, string httpMethod, string token = null);

        /// <summary>
        /// 服务器路径执行器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceIdOrPath"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        Task<T> InvokeAsync<T>(string serviceIdOrPath, IDictionary<string, object> paras);

        /// <summary>
        /// 最终执行器
        /// </summary>
        /// <param name="service"></param>
        /// <param name="paras"></param>
        /// <param name="token"></param>
        /// <param name="isDiscovery"></param>
        /// <returns></returns>
        Task<RemoteCallBackData> InvokeAsync(List<ServerAddress> service, string serviceIdOrPath, IDictionary<string, object> paras, string token);
    }
}
