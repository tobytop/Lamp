using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Core.Server;
using Lamp.Core.Server.Discovery;
using Lamp.Core.Server.ServiceContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lamp.Server.Transport
{
    public class RpcServer : IServer
    {
        private readonly List<ServiceRoute> _serviceRoutes = new List<ServiceRoute>();
        //执行容器
        private readonly IServiceEntryContainer _serviceEntryContainer;

        private IChannel _channel;
        private readonly ServerAddress _address;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private readonly IServiceDiscovery _serviceDiscovery;

        private readonly Stack<Func<RequestDel, RequestDel>> _middlewares;

        public RpcServer(ServerAddress address, IServiceEntryContainer serviceEntryContainer, ISerializer serializer, ILogger logger, IServiceDiscovery serviceDiscovery)
        {
            _address = address;
            _serializer = serializer;
            _logger = logger;
            _serviceEntryContainer = serviceEntryContainer;

            _serviceDiscovery = serviceDiscovery;
            //中间件执行委托
            _middlewares = new Stack<Func<RequestDel, RequestDel>>();
        }

        private async Task OnReceived(IChannelHandlerContext channel, TransportMsg message)
        {
            _logger.Debug($"开始触发服务: {message.Id}");
            if (message.ContentType == typeof(RemoteCallData).FullName)
            {
                IResponse response = new RpcResponse(channel, _serializer, _logger);
                RemoteExecutorContext thisContext = new RemoteExecutorContext(message, _serviceEntryContainer, response, _serializer, _logger, _serviceDiscovery);

                RequestDel lastInvoke = new RequestDel(async context =>
                {
                    RemoteCallBackData resultMessage = new RemoteCallBackData();
                    if (context.ServiceEntry == null)
                    {
                        resultMessage.ExceptionMessage = $"没有此服务：{context.RemoteInvokeMessage.ServiceId}";
                        await response.WriteAsync(message.Id, resultMessage);
                    }
                    else if (context.ServiceEntry.Descriptor.WaitExecution)
                    {
                        await LocalServiceExecuteAsync(context, resultMessage);
                        await response.WriteAsync(message.Id, resultMessage);
                    }
                    else
                    {
                        await response.WriteAsync(message.Id, resultMessage);
                        await Task.Factory.StartNew(async () =>
                        {
                            await LocalServiceExecuteAsync(context, resultMessage);
                        });
                    }
                });

                foreach (Func<RequestDel, RequestDel> middleware in _middlewares)
                {
                    lastInvoke = middleware.Invoke(lastInvoke);
                }
                await lastInvoke.Invoke(thisContext);

            }
            else
            {
                _logger.Debug($"msg: {message.Id}, message type is not an  RemoteCallData.");
            }
        }

        /// <summary>
        /// 本地执行方法
        /// </summary>
        /// <param name="serviceEntry"></param>
        /// <param name="invokeMessage"></param>
        /// <param name="resultMessage"></param>
        /// <returns></returns>
        private async Task LocalServiceExecuteAsync(RemoteExecutorContext context, RemoteCallBackData resultMessage)
        {
            try
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                if (!cancelTokenSource.IsCancellationRequested)
                {
                    object result = await context.ServiceEntry.Func(context);
                    Task task = result as Task;
                    if (task == null)
                    {
                        resultMessage.Result = result;
                    }
                    else
                    {
                        task.Wait(cancelTokenSource.Token);
                        TypeInfo taskType = task.GetType().GetTypeInfo();
                        if (taskType.IsGenericType)
                        {
                            resultMessage.Result = taskType.GetProperty("Result")?.GetValue(task);
                        }
                    }
                    resultMessage.ResultType = context.ServiceEntry.Descriptor.ReturnDesc;

                }
            }
            catch (Exception ex)
            {
                _logger.Error("throw exception when excuting local service: " + context.ServiceEntry.Descriptor.Id, ex);
                resultMessage.ExceptionMessage = ex.Message;
            }
        }

        public List<ServiceRoute> GetServiceRoutes()
        {
            if (!_serviceRoutes.Any())
            {
                List<ServiceEntry> serviceEntries = _serviceEntryContainer.GetServiceEntry();
                serviceEntries.ForEach(entry =>
                {
                    ServiceRoute serviceRoute = new ServiceRoute
                    {
                        Address = new List<ServerAddress> {
                             _address
                            },
                        ServiceDescriptor = entry.Descriptor
                    };
                    _serviceRoutes.Add(serviceRoute);
                });
            }

            return _serviceRoutes;
        }

        public async Task StartAsync()
        {
            DispatcherEventLoopGroup bossGroup = new DispatcherEventLoopGroup();
            WorkerEventLoopGroup workerGroup = new WorkerEventLoopGroup(bossGroup, 4);
            try
            {
                ServerBootstrap bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup)
                    .Channel<TcpServerChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new LoggingHandler("SRV-CONN"));

                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(4));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));

                        pipeline.AddLast(new ReadServerMessageChannelHandler(_serializer, _logger));
                        pipeline.AddLast("echo", new RpcServerHandler(async (context, message) =>
                        {
                            await OnReceived(context, message);
                        }));
                    }));

                _channel = await bootstrap.BindAsync(_address.CreateEndPoint());

                _logger.Info("服务器启动成功");
            }
            catch (Exception e)
            {
                _logger.Error($"错误日志: {e.Message}", e);
            }
        }

        public IServer Use(Func<RequestDel, RequestDel> middleware)
        {
            _middlewares.Push(middleware);
            return this;
        }
    }
}
