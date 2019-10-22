using Autofac;
using Lamp.Core.Client.IdentityServerExtension.Implement;
using Lamp.Core.Common.Logger;
using System;
using System.Collections.Generic;

namespace Lamp.Core.Client.IdentityServerExtension
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseIdentityServer(this IServiceHostClientBuilder serviceHostBuilder, Func<string, string, IDictionary<string, object>> getIdentityServerContext)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<AuthorizationHandler>().As<IAuthorizationHandler>().SingleInstance();
            });

            serviceHostBuilder.AddInitializer(componentRegister =>
            {
                IAuthorizationHandler context = componentRegister.Resolve<IAuthorizationHandler>();
                context.GetAuthorizationContext = getIdentityServerContext;


                ILogger logger = componentRegister.Resolve<ILogger>();
                logger.Info($"[config]identityserverExtension is set");
            });

            return serviceHostBuilder;
        }
    }
}
