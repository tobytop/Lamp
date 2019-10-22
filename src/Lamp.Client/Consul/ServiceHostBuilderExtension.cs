using Autofac;
using Lamp.Core.Client;
using Lamp.Core.Client.Discovery;
using Lamp.Core.Protocol.Server;
using System.Threading.Tasks;

namespace Lamp.Client.Consul
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseConsulForDiscovery(this IServiceHostClientBuilder serviceHostBuilder, RegisterServer registerServer, params ServerAddress[] address)
        {
            serviceHostBuilder.RegisterService(cb =>
            {
                cb.RegisterType<ConsulClientDiscovery>().As<IClientServiceDiscovery>().WithParameter("registerServer", registerServer).SingleInstance();
            });

            serviceHostBuilder.AddInitializer(container =>
            {
                IClientServiceDiscovery clientDiscovery = container.Resolve<IClientServiceDiscovery>();

                foreach (ServerAddress addr in address)
                {
                    clientDiscovery.AddRoutesGetter(() =>
                    {
                        ServerDesc serverDesc = new ServerDesc
                        {
                            ServerAddress = addr
                        };
                        return Task.FromResult(serverDesc);
                    });
                }
            });
            return serviceHostBuilder;
        }
    }
}
