using Autofac;
using Lamp.Core.Client;
using Lamp.Core.Client.LoadBalance;
using Lamp.Core.Common.Logger;
using static Lamp.Client.LoadBalance.DefaultAddressSelector;

namespace Lamp.Client.LoadBalance
{
    public static partial class ServiceHostBuilderExtension
    {
        /// <summary>
        ///  选择负载均衡
        /// </summary>
        /// <param name="serviceHostBuilder"></param>
        /// <returns></returns>
        public static IServiceHostClientBuilder UsePollingAddressSelector(this IServiceHostClientBuilder serviceHostBuilder, BalanceType balanceType)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                switch (balanceType)
                {
                    case BalanceType.RoundRobin:
                        containerBuilder.RegisterType<RoundRobin>().As<ILoadBalanceStrategy>().SingleInstance();
                        break;
                    case BalanceType.WeightRandom:
                        containerBuilder.RegisterType<WeightRandom>().As<ILoadBalanceStrategy>().SingleInstance();
                        break;
                    default:
                        containerBuilder.RegisterType<RoundRobin>().As<ILoadBalanceStrategy>().SingleInstance();
                        break;
                }

                containerBuilder.RegisterType<DefaultAddressSelector>().As<IAddressSelector>().WithParameter("balanceType", balanceType).SingleInstance();
            });

            serviceHostBuilder.AddInitializer(container =>
            {
                ILogger logger = container.Resolve<ILogger>();
                logger.Info($"采用默认的地址选择器");
            });
            return serviceHostBuilder;
        }
    }
}
