using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lamp.Client.ApiGateway;
using Lamp.Core.Client;
using Lamp.Core.Client.Discovery;
using Lamp.Swagger;
using Microsoft.Extensions.DependencyInjection;

namespace Lamp.Client
{
    public static class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseSwaggerAndGateWay(this IServiceHostClientBuilder serviceHostBuilder, RpcSwaggerOptions options, IServiceCollection services)
        {
            serviceHostBuilder.AddInitializer(container =>
            {
                //添加swagger
                services.UseRpcSwagger(options, container.Resolve<IClientServiceDiscovery>());
                //添加网关
                services.UseGateWay();
                //重新刷新容器
                var bulider = new ContainerBuilder();
                bulider.Populate(services);
                
                bulider.Update(container);
            });
            return serviceHostBuilder;
        }
    }
}
