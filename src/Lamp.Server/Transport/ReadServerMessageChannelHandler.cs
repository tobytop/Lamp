using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Serializer;
using MessagePack;
using System;

namespace Lamp.Server.Transport
{
    public class ReadServerMessageChannelHandler : ChannelHandlerAdapter
    {
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        public ReadServerMessageChannelHandler(ISerializer serializer, ILogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            IByteBuffer buffer = (IByteBuffer)message;
            try
            {
                byte[] data = new byte[buffer.ReadableBytes];
                buffer.GetBytes(buffer.ReaderIndex, data);

                string datajson = LZ4MessagePackSerializer.ToJson(data);
                var convertedMsg = _serializer.Deserialize<string, TransportMsg>(datajson);

                if (convertedMsg.ContentType == typeof(RemoteCallData).FullName)
                {
                    convertedMsg.Content = _serializer.Deserialize<object, RemoteCallData>(convertedMsg.Content);
                }
                else if (convertedMsg.ContentType == typeof(RemoteCallBackData).FullName)
                {
                    convertedMsg.Content = _serializer.Deserialize<object, RemoteCallBackData>(convertedMsg.Content);
                }
                context.FireChannelRead(convertedMsg);
            }
            catch (Exception e)
            {
                _logger.Error("转化错误", e);
            }
            finally
            {
                buffer.Release();
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }
    }
}
