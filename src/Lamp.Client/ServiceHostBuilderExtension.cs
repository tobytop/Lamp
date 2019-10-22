using Autofac;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Lamp.Client.Transport;
using Lamp.Core.Client;
using Lamp.Core.Client.Transport;
using Lamp.Core.Client.Transport.Implement;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Transport.Implement;
using System.Net;

namespace Lamp.Client
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseRpcForTransfer(this IServiceHostClientBuilder serviceHostBuilder)
        {

            serviceHostBuilder.AddInitializer(container =>
            {
                ITransportClientFactory factory = container.Resolve<ITransportClientFactory>();
                ILogger logger = container.Resolve<ILogger>();
                ISerializer serializer = container.Resolve<ISerializer>();
                Bootstrap bootstrap = new Bootstrap();

                logger.Info($"启动rpc客户端");

                bootstrap
                    .Group(new MultithreadEventLoopGroup())
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new LengthFieldPrepender(4));
                        pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                        pipeline.AddLast(new ReadClientMessageChannelHandler(serializer));
                        pipeline.AddLast(new RpcClientHandler(factory));
                    }));
                AttributeKey<IClientSender> clientSenderKey = AttributeKey<IClientSender>.ValueOf(typeof(DefaultTransportClientFactory), nameof(IClientSender));
                AttributeKey<IClientListener> clientListenerKey = AttributeKey<IClientListener>.ValueOf(typeof(DefaultTransportClientFactory), nameof(IClientListener));

                factory.ClientCreatorDelegate += (ServerAddress address, ref ITransportClient client) =>
                {
                    if (client == null && address.ServerFlag == ServerFlag.Rpc)
                    {
                        EndPoint ep = address.CreateEndPoint();
                        IChannel channel = bootstrap.ConnectAsync(ep).Result;
                        RpcClientListener listener = new RpcClientListener();
                        channel.GetAttribute(clientListenerKey).Set(listener);
                        RpcClientSender sender = new RpcClientSender(channel, serializer);
                        channel.GetAttribute(clientSenderKey).Set(sender);
                        client = new DefaultTransportClient(listener, sender, serializer, logger);
                    }
                };
            });

            return serviceHostBuilder;
        }
    }
}
