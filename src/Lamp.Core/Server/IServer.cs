using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Core.Server
{
    public delegate Task RequestDel(RemoteExecutorContext context);

    public interface IServer
    {
        /// <summary>
        ///  获取服务器上所有路径
        /// </summary>
        /// <returns></returns>
        List<ServiceRoute> GetServiceRoutes();

        /// <summary>
        ///   开启服务器
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        ///  在请求之后加中间件
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        IServer Use(Func<RequestDel, RequestDel> middleware);
    }
}
