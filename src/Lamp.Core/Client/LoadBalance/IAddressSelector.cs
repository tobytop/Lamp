using Lamp.Core.Protocol.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Core.Client.LoadBalance
{
    /// <summary>
    /// 服务器选择器
    /// </summary>
    public interface IAddressSelector
    {
        Task<ServerAddress> GetAddressAsync(List<ServerAddress> serverDesc);
    }
}
