using Autofac;
using Microsoft.Extensions.Configuration;
using System;

namespace Lamp.Server.Config
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder LoadConifg(this IServiceHostServerBuilder serviceHostBuilder, string FileName)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                string ProcessDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string path = ProcessDirectory + FileName;

                containerBuilder.Register(x => new ConfigurationBuilder().SetBasePath(ProcessDirectory).AddJsonFile(FileName).Build()).SingleInstance();
            });

            return serviceHostBuilder;
        }
    }
}
