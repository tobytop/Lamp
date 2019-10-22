using Autofac;
using Lamp.Core;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Server.ServiceContainer;
using Lamp.Core.ServiceId;
using System;

namespace Lamp.Server
{
    public class ServiceHostServerBuilder : ServiceHostBuilderBase, IServiceHostServerBuilder
    {
        public ServerAddress Address { get; set; }

        public ServiceHostServerBuilder(ContainerBuilder containerBuilder) : base(containerBuilder)
        {
            RegisterService(cb =>
            {
                cb.RegisterType<ServiceEntryContainer>().As<IServiceEntryContainer>().SingleInstance();
                cb.RegisterType<ServiceIdByClassGenerator>().As<IServiceIdGenerator>().SingleInstance();
            });
        }


        public new IServiceHostServerBuilder RegisterService(Action<ContainerBuilder> serviceRegister)
        {
            return base.RegisterService(serviceRegister) as IServiceHostServerBuilder;
        }

        public new IServiceHostServerBuilder AddInitializer(Action<IContainer> initializer)
        {
            return base.AddInitializer(initializer) as IServiceHostServerBuilder;
        }

        public new IServiceHostServerBuilder AddRunner(Action<IContainer> runner)
        {
            return base.AddRunner(runner) as IServiceHostServerBuilder;
        }
    }
}
