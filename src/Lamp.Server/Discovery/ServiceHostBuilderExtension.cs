using Autofac;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Server;
using Lamp.Core.Server.Discovery;
using Lamp.Core.Server.ServiceContainer;
using System.Collections.Generic;
using System.Threading;

namespace Lamp.Server.Discovery
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder UseInServerForDiscovery(this IServiceHostServerBuilder serviceHostBuilder)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<InServerServiceDiscovery>().As<IServiceDiscovery>().SingleInstance();
                containerBuilder.RegisterType<InServerServiceDiscovery>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
            });

            serviceHostBuilder.AddInitializer(async container =>
            {
                ILogger logger = container.Resolve<ILogger>();
                logger.Info($"本地服务器为注册服务器");
                while (!container.IsRegistered<IServer>())
                {
                    default(SpinWait).SpinOnce();
                }
                IServer server = container.Resolve<IServer>();
                IServiceEntryContainer entryContainer = container.Resolve<IServiceEntryContainer>();
                // 添加一个获取所有服务路径的服务
                entryContainer.AddServices(new[] { typeof(InServerServiceDiscovery) });

                List<ServiceRoute> routes = server.GetServiceRoutes();
                IServiceDiscovery discovery = container.Resolve<IServiceDiscovery>();
                await discovery.ClearAsync();
                await discovery.SetRoutesAsync(routes);

            });
            return serviceHostBuilder;
        }
    }
}
