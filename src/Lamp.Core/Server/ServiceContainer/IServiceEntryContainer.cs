using System;
using System.Collections.Generic;
using System.Text;

namespace Lamp.Core.Server.ServiceContainer
{
    public interface IServiceEntryContainer
    {
        /// <summary>
        ///  获取服务执行器
        /// </summary>
        /// <returns></returns>
        List<ServiceEntry> GetServiceEntry();

        /// <summary>
        /// 添加服务到服务容器
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        IServiceEntryContainer AddServices(Type[] types);
    }
}
