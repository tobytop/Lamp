using System;
using System.Net;

namespace Lamp.Core.Protocol.Server
{
    public class ServerAddress : ICloneable
    {
        protected ServerAddress(ServerFlag serverFlag)
        {
            ServerFlag = serverFlag;
        }

        public ServerAddress(string ip, int port, int weight = 2)
        {
            Ip = ip;
            Port = port;
            Weight = weight;
        }
        /// <summary>
        ///  健康检查
        /// </summary>
        public bool IsHealth { get; set; } = true;

        public string Ip { get; set; }

        public int Port { get; set; }

        /// <summary>
        /// 指示路径是否需要验证
        /// </summary>
        public bool EnableAuthorization { get; set; }

        public string Roles { get; set; }

        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 服务代码（IP+Port）
        /// </summary>
        public string Code => ToString();

        /// <summary>
        /// 服务性质
        /// </summary>
        public ServerFlag ServerFlag { get; set; } = ServerFlag.Rpc;

        public virtual EndPoint CreateEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(Ip), Port);
        }

        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public enum ServerFlag
    {
        Rpc = 1,
        Http = 2
    }
}
