using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Serializer;
using Lamp.Core.Server;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace Lamp.Server.Transport
{
    public class RpcResponse : IResponse
    {
        private readonly IChannelHandlerContext _channel;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        public RpcResponse(IChannelHandlerContext channel, ISerializer serializer, ILogger logger)
        {
            _channel = channel;
            _serializer = serializer;
            _logger = logger;
        }
        public async Task WriteAsync(string messageId, RemoteCallBackData resultMessage)
        {
            try
            {
                _logger.Debug($"结束处理任务: {messageId}");
                TransportMsg transportMsg = new TransportMsg
                {
                    Id = messageId,
                    Content = resultMessage,
                    ContentType = resultMessage.GetType().ToString()
                };
                var data = LZ4MessagePackSerializer.FromJson(_serializer.Serialize<string>(transportMsg));
                IByteBuffer buffer = Unpooled.Buffer(data.Length, data.Length);
                buffer.WriteBytes(data);
                await _channel.WriteAndFlushAsync(buffer);
            }
            catch (Exception ex)
            {
                _logger.Error("抛出错误 消息为: " + messageId, ex);
            }
        }
    }
}
