using Autofac;
using Lamp.Core.Client.Token.Implement;
using Lamp.Core.Common.Logger;
using System;

namespace Lamp.Core.Client.Token
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseToken(this IServiceHostClientBuilder serviceHostBuilder, Func<string> getToken)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<ServiceTokenGetter>().As<IServiceTokenGetter>().SingleInstance();
            });

            serviceHostBuilder.AddInitializer(componentRegister =>
            {
                IServiceTokenGetter tokenGetter = componentRegister.Resolve<IServiceTokenGetter>();
                tokenGetter.GetToken = getToken;


                ILogger logger = componentRegister.Resolve<ILogger>();
                logger.Info($"[config]get token has been set");
            });

            return serviceHostBuilder;
        }
    }
}
