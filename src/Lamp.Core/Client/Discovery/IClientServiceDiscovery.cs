using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Core.Client.Discovery
{
    public interface IClientServiceDiscovery
    {
        void AddRoutesGetter(Func<Task<ServerDesc>> getter);
        /// <summary>
        ///  获取所有路径
        /// </summary>
        /// <returns></returns>
        Task<List<ServerDesc>> GetRoutesAsync();

        /// <summary>
        ///  获取服务器地址
        /// </summary>
        /// <returns></returns>
        Task<List<ServerAddress>> GetAddressAsync();

    }
}
