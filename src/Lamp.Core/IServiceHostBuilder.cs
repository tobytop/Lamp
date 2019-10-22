using Autofac;
using System;

namespace Lamp.Core
{
    public interface IServiceHostBuilder
    {
        IServiceHostBuilder RegisterService(Action<ContainerBuilder> serviceRegister);

        IServiceHostBuilder AddInitializer(Action<IContainer> initializer);

        IServiceHostBuilder AddRunner(Action<IContainer> runner);

        IServiceHost Build();
    }
}
