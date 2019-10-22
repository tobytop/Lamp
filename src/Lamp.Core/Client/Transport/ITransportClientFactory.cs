using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Concurrent;

namespace Lamp.Core.Client.Transport
{
    /// <summary>
    /// 新建事件委托
    /// </summary>
    /// <param name="add"></param>
    /// <param name="client"></param>
    public delegate void CreatorDelegate(ServerAddress add, ref ITransportClient client);

    public interface ITransportClientFactory
    {
        /// <summary>
        /// 所有客户端放在内存
        /// </summary>
        ConcurrentDictionary<string, Lazy<ITransportClient>> Clients { get; }

        /// <summary>
        /// 创建客户端事件
        /// </summary>
        event CreatorDelegate ClientCreatorDelegate;

        ITransportClient CreateClient<T>(T address) where T : ServerAddress;
    }
}
