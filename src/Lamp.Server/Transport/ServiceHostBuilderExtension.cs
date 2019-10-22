using Autofac;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Server;
using System.Net;
using System.Net.Sockets;

namespace Lamp.Server.Transport
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder UseRpcForTransfer(this IServiceHostServerBuilder serviceHostBuilder, int port, bool isDebug = true)
        {
            string ip = string.Empty;
            if (isDebug)
            {
                ip = "127.0.0.1";
            }
            else
            {
                string hostName = Dns.GetHostName();
                IPHostEntry IpEntry = Dns.GetHostEntry(hostName); 

                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        ip = IpEntry.AddressList[i].ToString();
                    }
                }
            }

            serviceHostBuilder.Address = new ServerAddress(ip, port);

            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<RpcServer>().As<IServer>().WithParameter("address", serviceHostBuilder.Address).SingleInstance();
            });

            serviceHostBuilder.AddInitializer(container =>
            {
                ILogger logger = container.Resolve<ILogger>();
                logger.Info($"Rpc服务器开始启动");
                IServer server = container.Resolve<IServer>();
                server.StartAsync();
                //action?.Invoke(server);
            });

            return serviceHostBuilder;
        }

    }
}
