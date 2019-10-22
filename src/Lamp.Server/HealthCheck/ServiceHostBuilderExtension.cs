using Autofac;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Server;
using Lamp.Core.Server.Discovery;
using Lamp.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lamp.Server.HealthCheck
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder UseConsulCheckHealth(this IServiceHostServerBuilder serviceHostBuilder, RegisterServer registerServer)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                List<NamedParameter> ListNamedParameter = new List<NamedParameter>() {
                    new NamedParameter("registerServer", registerServer),
                    new NamedParameter("address", serviceHostBuilder.Address),
                }; 
                containerBuilder.RegisterType<ConsulDiscovery>().As<IServiceDiscovery>().As<IHealthCheck>().WithParameters(ListNamedParameter).SingleInstance();
            });

            serviceHostBuilder.AddInitializer(container =>
            {
                while (!container.IsRegistered<IServer>())
                {
                    //default(SpinWait).SpinOnce();
                    Thread.Sleep(200);
                }
                

                ILogger logger = container.Resolve<ILogger>();
                logger.Info($"[config]use consul for services discovery, consul ip: {registerServer.Ip}:{registerServer.Port}");

                IServer server = container.Resolve<IServer>();
                List<ServiceRoute> routes = server.GetServiceRoutes();
                logger.Debug("running consul found routes count: " + routes.Count);

                try
                {
                    IServiceDiscovery discovery = container.Resolve<IServiceDiscovery>();
                    discovery.ClearAsync().Wait();
                    discovery.SetRoutesAsync(routes).Wait();
                }
                catch (Exception ex)
                {
                    logger.Error($"error occurred while connecting with consul, ensure consul is running.\r\n", ex);
                }

            });
            serviceHostBuilder.AddRunner(container =>
            {
                IServer server = container.Resolve<IServer>();
                IHealthCheck check = container.Resolve<IHealthCheck>();
                check.RunAsync();
            });
            return serviceHostBuilder;
        }
    }
}
