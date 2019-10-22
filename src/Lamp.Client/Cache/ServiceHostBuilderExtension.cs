using Autofac;
using Lamp.Core.Cache;
using Lamp.Core.Client;
using Lamp.Core.Protocol.Server;
using System.Collections.Generic;

namespace Lamp.Client.Cache
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseRemoteCache(this IServiceHostClientBuilder serviceHostBuilder, int capacity)
        {
            serviceHostBuilder.RegisterService(cb =>
            {
                cb.RegisterType<LRUCache<string, List<ServerAddress>>>().As<ICache<string, List<ServerAddress>>>().WithParameter("capacity", capacity).SingleInstance();
            });

            return serviceHostBuilder;
        }
    }
}
