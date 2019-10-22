using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Lamp.Core.Client.Transport;
using Lamp.Core.Protocol.Communication;
using Lamp.Transport.Implement;

namespace Lamp.Client.Transport
{
    public class RpcClientHandler : ChannelHandlerAdapter
    {
        private readonly ITransportClientFactory _factory;

        public RpcClientHandler(ITransportClientFactory factory)
        {
            _factory = factory;
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            //_factory.Clients.TryRemove(context.Channel.GetAttribute(AttributeKey<string>.ValueOf(typeof(DefaultTransportClientFactory), "addresscode")).Get(), out _);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            TransportMsg msg = message as TransportMsg;

            IClientListener listener = context.Channel.GetAttribute(AttributeKey<IClientListener>.ValueOf(typeof(DefaultTransportClientFactory), nameof(IClientListener))).Get();
            IClientSender sender = context.Channel.GetAttribute(AttributeKey<IClientSender>.ValueOf(typeof(DefaultTransportClientFactory), nameof(IClientSender))).Get();

            listener.Received(sender, msg);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }
    }
}
