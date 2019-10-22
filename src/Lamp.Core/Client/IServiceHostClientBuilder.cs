using Autofac;
using System;

namespace Lamp.Core.Client
{
    public interface IServiceHostClientBuilder : IServiceHostBuilder
    {
        new IServiceHostClientBuilder RegisterService(Action<ContainerBuilder> serviceRegister);
        new IServiceHostClientBuilder AddInitializer(Action<IContainer> initializer);
        new IServiceHostClientBuilder AddRunner(Action<IContainer> runner);
    }
}
