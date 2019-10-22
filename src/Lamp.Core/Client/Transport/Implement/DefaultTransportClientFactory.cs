using Lamp.Core.Client.Transport;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Concurrent;

namespace Lamp.Transport.Implement
{
    public class DefaultTransportClientFactory : ITransportClientFactory
    {
        private readonly ILogger _logger;

        public DefaultTransportClientFactory(ILogger logger)
        {
            Clients = new ConcurrentDictionary<string, Lazy<ITransportClient>>();
            _logger = logger;
        }

        public ConcurrentDictionary<string, Lazy<ITransportClient>> Clients { get; }


        public event CreatorDelegate ClientCreatorDelegate;

        public ITransportClient CreateClient<T>(T address) where T : ServerAddress
        {
            try
            {
                _logger.Debug($"创建传输客户端: {address.Code}");
                ITransportClient val = Clients.GetOrAdd($"{address.ServerFlag}-{address.Code}", ep => new Lazy<ITransportClient>(() =>
                {
                    ITransportClient client = null;
                    ClientCreatorDelegate?.Invoke(address, ref client);
                    return client;
                })).Value;
                _logger.Debug($"成功创建传输客户端: {address.Code}");
                return val;
            }
            catch (Exception ex)
            {
                Clients.TryRemove($"{address.ServerFlag}-{address.Code}", out _);
                _logger.Error($"创建传输客户端失败 : {address.Code}", ex);
                throw;
            }
        }
    }
}
