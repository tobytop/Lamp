using Lamp.Core.Protocol.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Core.Server.Discovery
{
    public interface IServiceDiscovery
    {
        /// <summary>
        /// 清除所有路径
        /// </summary>
        /// <returns></returns>
        Task ClearAsync();

        /// <summary>
        /// 清除一个服务路径
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        Task ClearServiceAsync(string serviceId);

        /// <summary>
        /// 注册路径
        /// </summary>
        /// <param name="routes"></param>
        /// <returns></returns>
        Task SetRoutesAsync(IEnumerable<ServiceRoute> routes);

        /// <summary>
        /// 注册路径
        /// </summary>
        /// <param name="routes"></param>
        /// <returns></returns>
        Task AddRouteAsync(List<ServiceRoute> routes);

        /// <summary>
        /// 获取全部路径
        /// </summary>
        /// <returns></returns>
        Task<List<ServiceRoute>> GetRoutesAsync();

        /// <summary>
        /// 获取地址
        /// </summary>
        /// <returns></returns>
        Task<List<ServerAddress>> GetAddressAsync();

    }
}
