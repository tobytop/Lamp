using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Serializer;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Lamp.Core.Client.Transport.Implement
{
    public class DefaultTransportClient : ITransportClient
    {
        private readonly IClientListener _listener;

        /// <summary>
        /// 设置线程任务组
        /// </summary>
        private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMsg>> _resultCallbackDic =
            new ConcurrentDictionary<string, TaskCompletionSource<TransportMsg>>();

        private readonly IClientSender _sender;

        private readonly ISerializer _serializer;

        private readonly ILogger _logger;

        public DefaultTransportClient(IClientListener listener, IClientSender sender, ISerializer serializer, ILogger logger)
        {
            _listener = listener;
            _sender = sender;
            _serializer = serializer;
            _logger = logger;

            _listener.OnReceived += ListenerOnReceived;
        }

        public void Dispose()
        {
            (_sender as IDisposable)?.Dispose();
            (_listener as IDisposable)?.Dispose();
            foreach (TaskCompletionSource<TransportMsg> task in _resultCallbackDic.Values)
            {
                task.TrySetCanceled();
            }
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<RemoteCallBackData> SendAsync(RemoteCallData data)
        {
            try
            {
                data.ServiceId = data.ServiceId.ToLower();
                TransportMsg transportMsg = new TransportMsg()
                {
                    Content = data,
                    ContentType = data.GetType().ToString()
                };
                Task<RemoteCallBackData> callbackTask = RegisterResultCallbackAsync(transportMsg.Id);
                try
                {
                    await _sender.SendAsync(transportMsg);
                    return await callbackTask;
                }
                catch (Exception ex)
                {
                    _logger.Error($"当连接服务端时发生错误, serviceid: {data.ServiceId}", ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"传输失败,serviceid: {data.ServiceId}", ex);
                throw;
            }
        }

        private Task ListenerOnReceived(IClientSender sender, TransportMsg message)
        {
            _logger.Debug($"收到消息,消息id: {message.Id}");

            if (!_resultCallbackDic.TryGetValue(message.Id, out TaskCompletionSource<TransportMsg> task))
            {
                return Task.CompletedTask;
            }

            if (message.ContentType != typeof(RemoteCallBackData).FullName)
            {
                return Task.CompletedTask;
            }

            task.SetResult(message);
            return Task.CompletedTask;
        }

        private async Task<RemoteCallBackData> RegisterResultCallbackAsync(string id)
        {
            TaskCompletionSource<TransportMsg> task = new TaskCompletionSource<TransportMsg>();
            _resultCallbackDic.TryAdd(id, task);
            try
            {
                TransportMsg result = await task.Task;
                return result.GetContent<RemoteCallBackData>(_serializer);
            }
            finally
            {
                _resultCallbackDic.TryRemove(id, out _);
            }
        }
    }
}
