using Lamp.Core.Cache;
using Lamp.Core.Client.Discovery;
using Lamp.Core.Client.IdentityServerExtension;
using Lamp.Core.Client.LoadBalance;
using Lamp.Core.Client.Token;
using Lamp.Core.Client.Transport;
using Lamp.Core.Common.Logger;
using Lamp.Core.Common.TypeConverter;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Core.Client.RemoteExecutor.Implement
{
    public class RemoteServiceExecutor : IRemoteServiceExecutor
    {
        private readonly IAddressSelector _addressSelector;
        private readonly ILogger _logger;
        private readonly IClientServiceDiscovery _serviceDiscovery;
        private readonly IServiceTokenGetter _serviceTokenGetter;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ITypeConvertProvider _typeConvertProvider;
        private readonly ISerializer _serializer;
        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly ICache<string, List<ServerAddress>> _cache;
        private int _retryTimes;

        public RemoteServiceExecutor(IClientServiceDiscovery serviceDiscovery, IAddressSelector addressSelector,
            IServiceTokenGetter serviceTokenGetter, ITransportClientFactory transportClientFactory,
            ITypeConvertProvider typeConvertProvider, ILogger logger, ISerializer serializer,
            IAuthorizationHandler authorizationHandler = null, ICache<string, List<ServerAddress>> cache = null, int retryTimes = -1)
        {
            _serviceDiscovery = serviceDiscovery;
            _addressSelector = addressSelector;
            _transportClientFactory = transportClientFactory;
            _typeConvertProvider = typeConvertProvider;
            _serviceTokenGetter = serviceTokenGetter;
            _authorizationHandler = authorizationHandler;
            _cache = cache;
            _logger = logger;
            _retryTimes = retryTimes;
            _serializer = serializer;
        }

        public async Task<T> InvokeAsync<T>(string serviceIdOrPath, IDictionary<string, object> paras)
        {
            _logger.Debug($"开始启动服务: {serviceIdOrPath}");
            RemoteCallBackData result = await InvokeAsync(serviceIdOrPath, paras);
            if (!string.IsNullOrEmpty(result.ExceptionMessage))
            {
                _logger.Debug($"执行服务: {serviceIdOrPath} 发生错误: {result.ExceptionMessage}");
                throw new Exception(result.ExceptionMessage);
            }
            if (result.Result == null)
            {
                _logger.Debug($"执行服务: {serviceIdOrPath} 返回空。");
                return default(T);
            }
            object value;
            if (result.Result is Task<T> task)
            {
                value = _typeConvertProvider.Convert(task.Result, typeof(T));
            }
            else
            {
                value = _typeConvertProvider.Convert(result.Result, typeof(T));
            }

            _logger.Debug($"服务执行完毕，路径为: {serviceIdOrPath}.");
            return (T)value;

        }

        public async Task<RemoteCallBackData> InvokeAsync(string serviceIdOrPath, IDictionary<string, object> paras, string httpMethod = "get", string token = null)
        {
            if (paras == null)
            {
                paras = new ConcurrentDictionary<string, object>();
            }

            if (!(_cache != null && _cache.TryGet(serviceIdOrPath + "_" + httpMethod.ToLower(), out List<ServerAddress> service)))
            {
                service = await GetServiceByPathAsync(serviceIdOrPath, httpMethod);
            }


            if (service == null || service.Count == 0)
            {
                return new RemoteCallBackData
                {
                    ErrorCode = "404",
                    ErrorMsg = $"路径为：{serviceIdOrPath}, 没有找到！"
                };
            }

            if (token == null && _serviceTokenGetter?.GetToken != null)
            {
                token = _serviceTokenGetter.GetToken();
            }

            RemoteCallBackData result = await InvokeAsync(service, serviceIdOrPath, paras, token);
            if (!string.IsNullOrEmpty(result.ExceptionMessage))
            {
                return new RemoteCallBackData
                {
                    ErrorCode = "400",
                    ErrorMsg = $"{serviceIdOrPath}, {result.ToErrorString()}"
                };
            }

            if (string.IsNullOrEmpty(result.ErrorCode) && string.IsNullOrEmpty(result.ErrorMsg))
            {
                return result;
            }

            if (int.TryParse(result.ErrorCode, out int erroCode) && erroCode > 200 && erroCode < 600)
            {
                return new RemoteCallBackData
                {
                    ErrorCode = result.ErrorCode,
                    ErrorMsg = $"{serviceIdOrPath}, {result.ToErrorString()}"
                };
            }

            return result;

        }

        public async Task<RemoteCallBackData> InvokeAsync(List<ServerAddress> service, string serviceIdOrPath, IDictionary<string, object> paras, string token)
        {
            ServerAddress desc = await _addressSelector.GetAddressAsync(service);

            if (paras == null)
            {
                paras = new ConcurrentDictionary<string, object>();
            }

            if (_retryTimes < 0)
            {
                _retryTimes = service.Count;
            }
            RemoteCallBackData result = null;
            Polly.Retry.RetryPolicy retryPolicy = Policy.Handle<Exception>()
                .RetryAsync(_retryTimes,
                    async (ex, count) =>
                    {
                        desc = await _addressSelector.GetAddressAsync(service);
                        _logger.Debug(
                            $"FaultHandling,retry times: {count},serviceId: {serviceIdOrPath},Address: {desc.Code},RemoteServiceCaller excute retry by Polly for exception {ex.Message}");
                    });
            Polly.Wrap.PolicyWrap<RemoteCallBackData> fallbackPolicy = Policy<RemoteCallBackData>.Handle<Exception>()
                .FallbackAsync(new RemoteCallBackData() { ErrorCode = "500", ErrorMsg = "error occur when communicate with server. server maybe have been down." })
                .WrapAsync(retryPolicy);
            return await fallbackPolicy.ExecuteAsync(async () =>
            {
                ITransportClient client = _transportClientFactory.CreateClient(desc);
                if (client == null)
                {
                    return new RemoteCallBackData
                    {
                        ErrorCode = "400",
                        ErrorMsg = "服务不可用"
                    };
                }

                _logger.Debug($"invoke: serviceId:{serviceIdOrPath}, parameters count: {paras.Count()}, token:{token}");

                Payload payload = new Payload();

                if (!string.IsNullOrEmpty(token) && _authorizationHandler != null && desc.EnableAuthorization)
                {
                    var authorizationContext = _authorizationHandler.GetAuthorizationContext(token, desc.Roles);
                    if (authorizationContext != null)
                    {
                        payload.Items = authorizationContext;
                    }
                    else
                    {
                        return new RemoteCallBackData
                        {
                            ErrorMsg = "没有权限",
                            ErrorCode = "401"
                        };
                    }
                }

                result = await client.SendAsync(new RemoteCallData
                {
                    Payload = payload,
                    Parameters = paras,
                    ServiceId = serviceIdOrPath,
                    Token = token,
                });
                return result;
            });
        }

        private async Task<List<ServerAddress>> GetServiceByPathAsync(string path, string httpMethod)
        {
            List<ServerDesc> routes = await _serviceDiscovery.GetRoutesAsync();

            var serverList = routes.Where(x => (x.ServiceDescriptor.Count(o => o.Id.Equals(path, StringComparison.InvariantCultureIgnoreCase) &&
              o.HttpMethod.Equals(httpMethod, StringComparison.InvariantCultureIgnoreCase)) > 0));

            if (serverList.Count() > 0)
            {
                var serviceDescriptor = serverList.First().ServiceDescriptor.First(o => o.Id.Equals(path, StringComparison.InvariantCultureIgnoreCase) &&
                                    o.HttpMethod.Equals(httpMethod, StringComparison.InvariantCultureIgnoreCase));

                List<ServerAddress> service = serverList.Select(x => x.ServerAddress).ToList();

                service.ForEach(x =>
                {
                    x.EnableAuthorization = serviceDescriptor.EnableAuthorization;
                    x.Roles = serviceDescriptor.Roles;
                });

                _cache?.Set(path + "_" + httpMethod.ToLower(), service);

                return service;
            }
            else
            {
                return null;
            }
        }

        private static string ParsePath(string path, IDictionary<string, object> paras)
        {
            if (!paras.Any())
            {
                return path;
            }

            path += "?";
            path = paras.Keys.Aggregate(path, (current, key) => current + (key + "=&"));
            path = path.TrimEnd('&');

            return path;
        }
    }
}
