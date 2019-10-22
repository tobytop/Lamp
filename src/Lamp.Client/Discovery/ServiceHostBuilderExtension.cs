using Autofac;
using Lamp.Core.Client;
using Lamp.Core.Client.Discovery;
using Lamp.Core.Client.Discovery.Implement;
using Lamp.Core.Client.RemoteExecutor;
using Lamp.Core.Common.Logger;
using Lamp.Core.Common.TypeConverter;
using Lamp.Core.Protocol;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using System.Collections.Generic;
using System.Text;

namespace Lamp.Client.Discovery
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseInServerForDiscovery(this IServiceHostClientBuilder serviceHostBuilder, params ServerAddress[] address)
        {
            serviceHostBuilder.RegisterService(cb =>
            {
                cb.RegisterType<ClientServiceDiscovery>().As<IClientServiceDiscovery>().SingleInstance();
            });

            serviceHostBuilder.AddInitializer(container =>
            {
                IClientServiceDiscovery clientDiscovery = container.Resolve<IClientServiceDiscovery>();
                IRemoteServiceExecutor remoteExecutor = container.Resolve<IRemoteServiceExecutor>();
                ISerializer serializer = container.Resolve<ISerializer>();
                ITypeConvertProvider typeConverter = container.Resolve<ITypeConvertProvider>();
                ILogger logger = container.Resolve<ILogger>();
                StringBuilder sb = new StringBuilder();

                foreach (ServerAddress addr in address)
                {
                    sb.AppendFormat(addr.Code + ",");
                    clientDiscovery.AddRoutesGetter(async () =>
                    {
                        RemoteCallBackData result = await remoteExecutor.InvokeAsync(new List<ServerAddress>() { addr }, "Lamp.ServiceDiscovery.InServer.GetRoutesDescAsync".ToLower(), null, null);
                        if (result == null || result.HasError)
                        {
                            return null;
                        }

                        List<ServiceRouteDesc> routesDesc = (List<ServiceRouteDesc>)typeConverter.Convert(result.Result, typeof(List<ServiceRouteDesc>));

                        ServerDesc server = new ServerDesc
                        {
                            ServerAddress = addr,
                            ServiceDescriptor = new List<ServiceDesc>()
                        };
                        server.ServerAddress.IsHealth = true;

                        foreach (ServiceRouteDesc desc in routesDesc)
                        {
                            ServiceDesc item = (ServiceDesc)desc.ServiceDescriptor.Clone();
                            server.ServiceDescriptor.Add(item);
                        }

                        return server;
                    });
                }
                if (sb.Length > 0)
                {
                    logger.Info($"[config]用服务端发现服务 {sb.ToString()}");
                }
            });

            serviceHostBuilder.AddRunner(container =>
            {
                ClientServiceDiscovery clientServiceDiscovery = (ClientServiceDiscovery)container.Resolve<IClientServiceDiscovery>();
                clientServiceDiscovery?.RunInInit().Wait();

            });

            return serviceHostBuilder;
        }
    }
}
