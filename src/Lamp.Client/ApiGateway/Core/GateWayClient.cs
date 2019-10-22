using Autofac;
using Lamp.Core;
using Lamp.Core.Client.RemoteExecutor;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Serializer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Client.ApiGateway.Core
{
    public class GateWayClient
    {
        public static async Task<RemoteCallBackData> Invoke(string path, IDictionary<string, object> paras, string httpMethod, IContainer container)
        {
            IRemoteServiceExecutor remoteServiceInvoker = container.Resolve<IRemoteServiceExecutor>();
            ISerializer converter = container.Resolve<ISerializer>();
            RemoteCallBackData result = await remoteServiceInvoker.InvokeAsync(path, paras, httpMethod);
            if (!string.IsNullOrEmpty(result.ExceptionMessage))
            {
                throw new HttpStatusCodeException(400, $"{result.ToErrorString()}", path);
            }

            if (!string.IsNullOrEmpty(result.ErrorCode) || !string.IsNullOrEmpty(result.ErrorMsg))
            {
                if (int.TryParse(result.ErrorCode, out int erroCode) && erroCode > 200 && erroCode < 600)
                {
                    throw new HttpStatusCodeException(erroCode, result.ToErrorString(), path);
                }

                return new RemoteCallBackData { ErrorCode = result.ErrorCode, ErrorMsg = result.ErrorMsg };
            }
            if (result.ResultType == typeof(FileData).ToString())
            {
                object file = converter.Deserialize(result.Result, typeof(FileData));
                result.Result = file;
            }

            return result;
        }
    }
}
