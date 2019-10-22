using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Generic;

namespace Lamp.Core.Filter
{
    public class FilterContext
    {
        /// <summary>
        /// 头部
        /// </summary>
        public Payload Payload { get; set; }

        /// <summary>
        /// 服务参数
        /// </summary>
        public Dictionary<Type, object> ServiceArguments { get; set; }

        /// <summary>
        /// 服务描述
        /// </summary>
        public ServiceDesc Descriptor { get; set; }

        /// <summary>
        ///结果
        /// </summary>
        public object Result { get; set; }

    }
}
