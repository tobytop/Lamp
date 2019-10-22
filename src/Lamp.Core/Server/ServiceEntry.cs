using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Lamp.Core.Server
{
    /// <summary>
    /// 服务执行器
    /// </summary>
    public class ServiceEntry
    {
        /// <summary>
        /// 服务执行器
        /// </summary>
        public Func<RemoteExecutorContext, Task<object>> Func { get; set; }

        /// <summary>
        ///  服务描述
        /// </summary>
        public ServiceDesc Descriptor { get; set; }

        /// <summary>
        /// 方法参数
        /// </summary>
        public ParameterInfo[] Parameters { get; set; }
    }
}
