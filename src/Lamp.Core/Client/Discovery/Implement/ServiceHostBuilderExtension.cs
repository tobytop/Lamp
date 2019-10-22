using Autofac;
using Lamp.Core.Common.Logger;

namespace Lamp.Core.Client.Discovery.Implement
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder SetDiscoveryAutoUpdateJobInterval(
            this IServiceHostClientBuilder serviceHostBuilder, int updateJobIntervalMinute)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<ClientServiceDiscovery>().As<IClientServiceDiscovery>().WithParameter("updateJobIntervalMinute", updateJobIntervalMinute).SingleInstance();
            });
            serviceHostBuilder.AddInitializer(container =>
            {
                ILogger logger = container.Resolve<ILogger>();
                logger.Info($"[config]services discovery auto update job interval: {updateJobIntervalMinute} min");
            });

            return serviceHostBuilder;
        }
    }
}
