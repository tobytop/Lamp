using Autofac;
using Lamp.Core.Common.Logger;
using Lamp.Core.Common.Logger.Implement;
using Lamp.Core.Common.TypeConverter;
using Lamp.Core.Serializer;
using System;
using System.Collections.Generic;

namespace Lamp.Core
{
    public abstract class ServiceHostBuilderBase : IServiceHostBuilder
    {
        /// <summary>
        ///  一些需要服务初始化的事件
        /// </summary>
        private readonly List<Action<IContainer>> _initializers;

        /// <summary>
        ///  需要在build后去触发的事件
        /// </summary>
        private readonly List<Action<IContainer>> _runners;

        /// <summary>
        /// 所有需要注册的服务容器
        /// </summary>
        private readonly List<Action<ContainerBuilder>> _serviceRegisters;

        private readonly ContainerBuilder _containerBuilder;

        protected ServiceHostBuilderBase(ContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder;
            _serviceRegisters = new List<Action<ContainerBuilder>>();
            _initializers = new List<Action<IContainer>>();
            _runners = new List<Action<IContainer>>();
        }

        public IServiceHost Build()
        {
            IContainer container = null;
            ServiceHost host = new ServiceHost(_runners);
            _containerBuilder.Register(x => host).As<IServiceHost>().SingleInstance();
            _containerBuilder.Register(x => container).As<IContainer>().SingleInstance();
            _containerBuilder.RegisterType<TypeConvertProvider>().As<ITypeConvertProvider>().SingleInstance();
            _containerBuilder.RegisterType<JasonSerializer>().As<ISerializer>().SingleInstance();
            _containerBuilder.RegisterType<SeriLogger>().As<ILogger>().SingleInstance();

            _serviceRegisters.ForEach(x => { x.Invoke(_containerBuilder); });

            container = _containerBuilder.Build();
            _initializers.ForEach(x => { x.Invoke(container); });

            host.Container = container;

            return host;
        }

        public IServiceHostBuilder AddInitializer(Action<IContainer> initializer)
        {
            _initializers.Add(initializer);
            return this;
        }

        public IServiceHostBuilder AddRunner(Action<IContainer> runner)
        {
            _runners.Add(runner);
            return this;
        }

        public IServiceHostBuilder RegisterService(Action<ContainerBuilder> serviceRegister)
        {
            _serviceRegisters.Add(serviceRegister);
            return this;
        }
    }
}
