using Autofac.Extensions.DependencyInjection;
using Lamp.Core.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Lamp.Client.ApiGateway
{
    /// <summary>
    /// 采用autofac做为第三方ioc 与swagger的拓展不能共用
    /// </summary>
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseAutofacForMvc(this IServiceHostClientBuilder serviceHostBuilder, IServiceCollection services)
        {
            serviceHostBuilder.RegisterService(cb =>
            {
                cb.Populate(services);
            });

            return serviceHostBuilder;
        }
    }
}
