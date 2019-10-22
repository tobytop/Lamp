using Lamp.Core.Client.LoadBalance;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lamp.Client.LoadBalance
{
    public class DefaultAddressSelector : AddressSelectorBase
    {
        //private readonly ConcurrentDictionary<string, Lazy<ServerIndexHolder>> _addresses =
        //    new ConcurrentDictionary<string, Lazy<ServerIndexHolder>>();

        private readonly ILogger _logger;
        private readonly ILoadBalanceStrategy _balanceStrategy;

        public DefaultAddressSelector(ILogger logger, ILoadBalanceStrategy balanceStrategy)
        {
            _logger = logger;
            _balanceStrategy = balanceStrategy;
        }


        public override Task<ServerAddress> GetAddressAsyn(List<ServerAddress> serverDesc)
        {
            ServerAddress desc;

            if (serverDesc.Count > 1)
            {
                desc = _balanceStrategy.DoLoadBalance(serverDesc);
            }
            else
            {
                desc = serverDesc[0];
            }

            _logger.Debug($"服务器选择器选择: {desc.Code}");
            return Task.FromResult(desc);
        }

        /// <summary>
        /// 轮询算法负载均衡
        /// </summary>
        public class RoundRobin : ILoadBalanceStrategy
        {
            private int _latestIndex;
            private int _lock;

            public ServerAddress DoLoadBalance(List<ServerAddress> addresses)
            {
                while (true)
                {
                    if (Interlocked.Exchange(ref _lock, 1) != 0)
                    {
                        default(SpinWait).SpinOnce();
                        continue;
                    }

                    _latestIndex = (_latestIndex + 1) % addresses.Count;
                    if (_latestIndex >= addresses.Count)
                    {
                        _latestIndex = 0;
                    }

                    ServerAddress address = addresses[_latestIndex];

                    Interlocked.Exchange(ref _lock, 0);
                    return address;
                }
            }
        }

        /// <summary>
        /// 随机权重负载均衡
        /// </summary>
        public class WeightRandom : ILoadBalanceStrategy
        {
            public ServerAddress DoLoadBalance(List<ServerAddress> addresses)
            {
                List<ServerAddress> list = new List<ServerAddress>();
                foreach (ServerAddress addr in addresses)
                {
                    for (int i = 0; i < addr.Weight; i++)
                    {
                        list.Add((ServerAddress)addr.Clone());
                    }
                }
                Random random = new Random();
                int randomPos = random.Next(list.Count());

                return list[randomPos];
            }
        }
    }
}
