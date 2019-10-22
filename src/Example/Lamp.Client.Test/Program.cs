using Autofac;
using Lamp.Client.Transport;
using Lamp.Core.Client.Transport;
using Lamp.Core.Client.Transport.Implement;
using Lamp.Core.Common.Logger;
using Lamp.Core.Common.Logger.Implement;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Transport.Implement;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lamp.Client.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(10000);
            var containerBuilder = new ContainerBuilder();
            var builder = new ServiceHostClientBuilder(containerBuilder).UseRpcForTransfer();
            using (var host = builder.Build())
            {
                ITransportClient client = host.Container.Resolve<ITransportClientFactory>()
                    .CreateClient(new ServerAddress("127.0.0.1", 8008));

                //ITransportClient client = container.Resolve<ITransportClientFactory>()
                //    .DecorationFactory(container.Resolve<ISerializer>())
                //    .CreateClient(new ServerAddress("127.0.0.1", 8007));
                RemoteCallData sendMessage = new RemoteCallData
                {
                    ServiceId = "base/fastdfs",
                    Parameters = new Dictionary<string, object>
                                {
                                    { "message", 12 },
                                    { "myout", 122}
                                }
                };

                RemoteCallBackData result = client.SendAsync(sendMessage).Result;
                Console.Write(result.Result.ToString());
                Console.ReadLine();
            }
        }
    }
}
