using Autofac;
using Lamp.Core;
using Lamp.Core.Protocol.Server;
using System;

namespace Lamp.Server
{
    public interface IServiceHostServerBuilder : IServiceHostBuilder
    {
        ServerAddress Address { get; set; }

        new IServiceHostServerBuilder RegisterService(Action<ContainerBuilder> serviceRegister);
        new IServiceHostServerBuilder AddInitializer(Action<IContainer> initializer);
        new IServiceHostServerBuilder AddRunner(Action<IContainer> runner);
    }
}
