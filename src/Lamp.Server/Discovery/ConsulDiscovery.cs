using Consul;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Core.Server.Discovery;
using Lamp.Server.HealthCheck;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lamp.Server.Discovery
{
    /// <summary>
    /// 使用Consul作为服务注册和服务发现
    /// </summary>
    public class ConsulDiscovery : IHealthCheck, IServiceDiscovery
    {
        private readonly ILogger _logger;
        private readonly ConsulClient _consulClient;
        private List<ServiceRoute> _routes;
        private readonly ISerializer _serializer;
        private readonly ServerAddress _address;
        private readonly IConfigurationRoot _configuration;

        public ConsulDiscovery(ILogger logger, ISerializer serializer, RegisterServer registerServer, ServerAddress address, IConfigurationRoot configuration = null)
        {
            _logger = logger;
            _serializer = serializer;
            _address = address;

            _configuration = configuration;
            _routes = new List<ServiceRoute>();
            _consulClient = new ConsulClient(config =>
            {
                config.Address = new Uri($"http://{registerServer.Ip}:{registerServer.Port}");
            });
        }

        public async Task AddRouteAsync(List<ServiceRoute> routes)
        {
            List<ServiceRoute> curRoutes = await GetRoutesAsync();
            foreach (ServiceRoute route in routes)
            {
                curRoutes.RemoveAll(x => x.ServiceDescriptor.Id == route.ServiceDescriptor.Id);
                curRoutes.Add(route);
            }
            await SetRoutesAsync(curRoutes);
        }

        public async Task ClearAsync()
        {
            await _consulClient.KV.Delete(GetKey());
        }

        public async Task ClearServiceAsync(string serviceId)
        {
            List<ServiceRoute> routes = await GetRoutesAsync();
            int hasRemove = routes.RemoveAll(x => x.ServiceDescriptor.Id == serviceId);
            if (hasRemove > 0)
            {
                await SetRoutesAsync(routes);
            }
        }

        public async Task<List<ServerAddress>> GetAddressAsync()
        {
            List<ServiceRoute> routes = await GetRoutesAsync();
            List<ServerAddress> addresses = new List<ServerAddress>();
            if (routes != null && routes.Any())
            {
                addresses = routes.SelectMany(x => x.Address).Distinct().ToList();
            }
            return addresses;
        }

        public async Task<List<ServiceRoute>> GetRoutesAsync()
        {
            if (_routes != null && _routes.Any())
            {
                return _routes.ToList();
            }
            byte[] data = (await _consulClient.KV.Get(GetKey())).Response?.Value;
            if (data == null)
            {
                return _routes;
            }

            ServerDesc descriptor = _serializer.Deserialize<byte[], ServerDesc>(data);
            if (descriptor != null)
            {
                foreach (ServiceDesc desc in descriptor.ServiceDescriptor)
                {
                    _routes.Add(new ServiceRoute
                    {
                        ServiceDescriptor = desc.Clone() as ServiceDesc
                    });
                }
            }

            return _routes;
        }

        private string GetKey()
        {
            return $"{_address.Ip}-{_address.Port}";
        }

        public Task RunAsync()
        {
            QueryResult<Dictionary<string, AgentService>> res = _consulClient.Agent.Services().Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                ApplicationException ex = new ApplicationException($"Failed to query services");
                _logger.Error($"Failed to query services", ex);
                throw ex;
            }
            if (!res.Response.Values.Any(x => x.Address == _address.Ip && x.Port == _address.Port))
            {
                RegisterService(_address.Ip, _address.Port);
            }
            return Task.CompletedTask;
        }

        public async Task SetRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            _routes = routes.ToList();
            ServerDesc routeDescriptor = new ServerDesc()
            {
                ServerAddress = _address,
                ServiceDescriptor = new List<ServiceDesc>()
            };
            routeDescriptor.ServerAddress = _address;

            foreach (ServiceRoute route in routes)
            {
                routeDescriptor.ServiceDescriptor.Add(route.ServiceDescriptor);
            }

            //await SetRoutesAsync(routeDescriptors);
            byte[] nodeData = _serializer.Serialize<byte[]>(routeDescriptor);
            KVPair keyValuePair = new KVPair(GetKey()) { Value = nodeData };
            await _consulClient.KV.Put(keyValuePair);
        }

        private void RegisterService(string Ip, int Port)
        {
            string serviceId = $"{Ip}-{Port}";
            IEnumerable<IConfigurationSection> options = _configuration?.GetSection("ConsulOptions").GetChildren();

            ConsulConfig consulConfig = new ConsulConfig();
            if (options?.Count()>0)
            {
                consulConfig.CheckInterval = int.Parse(options.First(x => x.Key == "CheckInterval").Value);
                consulConfig.CriticalInterval = int.Parse(options.First(x => x.Key == "CriticalInterval").Value);
            }

            AgentCheckRegistration acr = new AgentCheckRegistration
            {
                TCP = $"{Ip}:{Port}",
                Name = serviceId,
                ID = serviceId,
                Interval = TimeSpan.FromSeconds(consulConfig.CheckInterval),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(consulConfig.CriticalInterval)
            };
            AgentServiceRegistration asr = new AgentServiceRegistration
            {
                Address = Ip,
                ID = serviceId,
                Name = serviceId,
                Port = Port,
                Check = acr
            };
            WriteResult res = _consulClient.Agent.ServiceRegister(asr).Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                ApplicationException ex = new ApplicationException($"Failed to register service {Ip} on port {Port}");
                _logger.Error($"Failed to register service {Ip} on port {Port}", ex);
                throw ex;
            }
        }

    }
}
