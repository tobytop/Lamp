using Autofac;
using Lamp.Client.LoadBalance;
using Lamp.Core.Client.LoadBalance;
using Lamp.Core.Client.Transport;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Client.Tool
{
    public static class ServerClient
    {
        /// <summary>
        /// 服务内部调用方法
        /// </summary>
        /// <param name="serverAddress">服务地址</param>
        /// <param name="sendMessage">请求内容</param>
        public static async Task<T> InternalCall<T>(List<ServerAddress> service, RemoteCallData sendMessage)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            Core.Client.IServiceHostClientBuilder builder = new ServiceHostClientBuilder(containerBuilder).UseRpcForTransfer()
                .UsePollingAddressSelector(BalanceType.RoundRobin);
            using (Core.IServiceHost host = builder.Build())
            {
                IAddressSelector addressSelector = host.Container.Resolve<IAddressSelector>();
                ServerAddress desc = await addressSelector.GetAddressAsync(service);
                ITransportClient client = host.Container.Resolve<ITransportClientFactory>()
                    .CreateClient(desc);

                RemoteCallBackData result = client.SendAsync(sendMessage).Result;
                return JsonConvert.DeserializeObject<T>(result.Result.ToString());
            }
        }
    }
}
