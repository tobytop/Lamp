using Autofac;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Attributes;
using Lamp.Core.Server.ServiceContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Lamp.Server
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder LoadServices(this IServiceHostServerBuilder serviceHostBuilder,
           params string[] assemblyNames)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string assemblyName in assemblyNames)
            {
                string name = assemblyName;
                if (name.EndsWith(".dll"))
                {
                    name = name.Substring(0, name.Length - 4);
                }

                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(name));
                assemblies.Add(assembly);
            }

            List<Type> serviceTypes = assemblies.SelectMany(x => x.ExportedTypes)
                .Where(x => x.GetMethods().Any(y => y.GetCustomAttribute<ServiceAttribute>() != null)).ToList();

            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterTypes(serviceTypes.ToArray()).AsSelf().AsImplementedInterfaces()
                    .InstancePerDependency();
                
                // 注册模块
                assemblies.ForEach(x => { containerBuilder.RegisterAssemblyModules(x); });
            });
            serviceHostBuilder.AddInitializer(container =>
            {
                IServiceEntryContainer serviceEntryContainer = container.Resolve<IServiceEntryContainer>();
                serviceEntryContainer.AddServices(serviceTypes.ToArray());

                ILogger logger = container.Resolve<ILogger>();
                logger.Info($"已经注册的服务: {string.Join(",", assemblies)}");
            });
            return serviceHostBuilder;
        }
    }
}
