using DotNetty.Transport.Channels;
using Lamp.Core.Protocol.Communication;
using System;
using System.Threading.Tasks;

namespace Lamp.Server.Transport
{
    public class RpcServerHandler : ChannelHandlerAdapter
    {
        private readonly Action<IChannelHandlerContext, TransportMsg> _readAction;

        public RpcServerHandler(Action<IChannelHandlerContext, TransportMsg> readAction)
        {
            _readAction = readAction;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Task.Run(() =>
            {
                TransportMsg msg = message as TransportMsg;
                _readAction(context, msg);
            });
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }
}
