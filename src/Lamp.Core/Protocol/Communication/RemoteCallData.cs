using System;
using System.Collections.Generic;

namespace Lamp.Core.Protocol.Communication
{
    /// <summary>
    /// RPC服务请求体
    /// </summary>
    [Serializable]
    public class RemoteCallData
    {
        /// <summary>
        /// 请求头部
        /// </summary>
        public Payload Payload { get; set; }

        /// <summary>
        /// 服务id
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 验证票据
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }
    }
}
