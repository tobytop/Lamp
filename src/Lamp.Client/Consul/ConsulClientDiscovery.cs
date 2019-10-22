using Consul;
using Lamp.Core.Client.Discovery;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lamp.Client.Consul
{
    public class ConsulClientDiscovery : IClientServiceDiscovery, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ISerializer _serializer;
        private readonly ConsulClient _consulClient;

        private readonly List<Func<Task<ServerDesc>>> _routesGetters;

        public ConsulClientDiscovery(ILogger logger, ISerializer serializer, RegisterServer registerServer)
        {
            _logger = logger;
            _serializer = serializer;
            _routesGetters = new List<Func<Task<ServerDesc>>>();
            _consulClient = new ConsulClient(config =>
            {
                config.Address = new Uri($"http://{registerServer.Ip}:{registerServer.Port}");
            });
        }
        public void AddRoutesGetter(Func<Task<ServerDesc>> getter)
        {
            _routesGetters.Add(getter);
        }

        public void Dispose()
        {
            _consulClient.Dispose();
        }

        public Task<List<ServerAddress>> GetAddressAsync()
        {
            QueryResult<Dictionary<string, AgentService>> res = _consulClient.Agent.Services().Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                ApplicationException ex = new ApplicationException($"不能查询服务");
                _logger.Error($"不能查询服务", ex);
                throw ex;
            }
            Dictionary<string, AgentService>.ValueCollection result = res.Response.Values;
            List<ServerAddress> addresses = new List<ServerAddress>();
            foreach (AgentService r in result)
            {
                addresses.Add(new ServerAddress(r.Address, r.Port));
            }
            return Task.FromResult(addresses);
        }

        public async Task<List<ServerDesc>> GetRoutesAsync()
        {
            List<ServerAddress> addresses = await GetAddressAsync();
            List<ServerDesc> reuslt = new List<ServerDesc>(_routesGetters.Count);
            foreach (Func<Task<ServerDesc>> getter in _routesGetters)
            {
                ServerDesc server = await getter();
                if (addresses.Where(x => x.Ip == server.ServerAddress.Ip && x.Port == server.ServerAddress.Port).Any())
                {
                    byte[] data = (await _consulClient.KV.Get($"{server.ServerAddress.Ip}-{server.ServerAddress.Port}")).Response?.Value;
                    if (data == null)
                    {
                        continue;
                    }
                    ServerDesc descriptors = _serializer.Deserialize<byte[], ServerDesc>(data);
                    reuslt.Add(descriptors);
                }
            }
            return reuslt;
        }
    }
}
