using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Lamp.Core.Client.Transport;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Serializer;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace Lamp.Client.Transport
{
    public class RpcClientSender : IClientSender, IDisposable
    {
        private readonly IChannel _channel;

        private readonly ISerializer _serializer;

        public RpcClientSender(IChannel channel, ISerializer serializer)
        {
            _channel = channel;
            _serializer = serializer;
        }
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        public async Task SendAsync(TransportMsg message)
        {
            IByteBuffer buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        private IByteBuffer GetByteBuffer(TransportMsg message)
        {
            var data = LZ4MessagePackSerializer.FromJson(_serializer.Serialize<string>(message));
            IByteBuffer buffer = Unpooled.Buffer(data.Length, data.Length);
            return buffer.WriteBytes(data);
        }
    }
}
