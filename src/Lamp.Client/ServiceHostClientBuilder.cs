using Autofac;
using Lamp.Client.Transport;
using Lamp.Core;
using Lamp.Core.Client;
using Lamp.Core.Client.RemoteExecutor;
using Lamp.Core.Client.RemoteExecutor.Implement;
using Lamp.Core.Client.Transport;
using Lamp.Transport.Implement;
using System;

namespace Lamp.Client
{
    public class ServiceHostClientBuilder : ServiceHostBuilderBase, IServiceHostClientBuilder
    {
        public ServiceHostClientBuilder(ContainerBuilder containerBuilder) : base(containerBuilder)
        {
            RegisterService(cb =>
            {
                cb.RegisterType<RemoteServiceExecutor>().As<IRemoteServiceExecutor>().SingleInstance();
                cb.RegisterType<DefaultTransportClientFactory>().As<ITransportClientFactory>().SingleInstance();
                cb.RegisterType<RpcClientSender>().As<IClientSender>().SingleInstance();
                cb.RegisterType<RpcClientListener>().As<IClientListener>().SingleInstance();
            });
        }


        public new IServiceHostClientBuilder RegisterService(Action<ContainerBuilder> serviceRegister)
        {
            return base.RegisterService(serviceRegister) as IServiceHostClientBuilder;
        }

        public new IServiceHostClientBuilder AddInitializer(Action<IContainer> initializer)
        {
            return base.AddInitializer(initializer) as IServiceHostClientBuilder;
        }

        public new IServiceHostClientBuilder AddRunner(Action<IContainer> runner)
        {
            return base.AddRunner(runner) as IServiceHostClientBuilder;
        }
    }
}
