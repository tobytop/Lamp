using Lamp.Core.Protocol;
using Lamp.Core.Protocol.Attributes;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Core.Server.Discovery;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Server.Discovery
{
    public class InServerServiceDiscovery : IServiceDiscovery
    {
        private static readonly List<ServiceRoute> _routes = new List<ServiceRoute>();

        private readonly ISerializer _serializer;
        public InServerServiceDiscovery(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public Task ClearAsync()
        {
            _routes.Clear();
            return Task.CompletedTask;
        }

        public Task ClearAsync(string address)
        {
            _routes.Clear();
            return Task.CompletedTask;
        }

        public Task ClearServiceAsync(string serviceId)
        {
            _routes.Remove(_routes.FirstOrDefault(x => x.ServiceDescriptor.Id == serviceId));
            return Task.CompletedTask;
        }

        public Task SetRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            //剔除老的路径
            _routes.RemoveAll(x => routes.Any(y => y.ServiceDescriptor.Id == x.ServiceDescriptor.Id));
            //添加新的路径
            _routes.AddRange(routes.ToList());
            return Task.CompletedTask;
        }

        public Task AddRouteAsync(List<ServiceRoute> routes)
        {
            foreach (ServiceRoute route in routes)
            {
                _routes.RemoveAll(x => x.ServiceDescriptor.Id == route.ServiceDescriptor.Id);
                _routes.Add(route);
            }
            return Task.CompletedTask;
        }

        public Task<List<ServiceRoute>> GetRoutesAsync()
        {
            return Task.FromResult(_routes);
        }

        [Service(Id = "Lamp.ServiceDiscovery.InServer.GetRoutesDescAsync", Comment = "get all service routes in this server")]
        public Task<List<ServiceRouteDesc>> GetRoutesDescAsync()
        {
            List<ServiceRouteDesc> routeDescriptors = new List<ServiceRouteDesc>(_routes.Count());
            foreach (ServiceRoute route in _routes)
            {
                routeDescriptors.Add(new ServiceRouteDesc
                {
                    ServiceDescriptor = route.ServiceDescriptor,
                    AddressDescriptors = route.Address?.Select(x => new AddressDesc
                    {
                        Type = $"{x.GetType().FullName}, {x.GetType().Assembly.GetName()}",
                        Value = _serializer.Serialize<string>(x)
                    })
                });
            }

            return Task.FromResult(routeDescriptors);
        }

        public Task<List<ServerAddress>> GetAddressAsync()
        {
            return Task.FromResult(_routes.SelectMany(x => x.Address).Distinct().ToList());
        }
    }
}
