using Lamp.Core.Protocol.Communication;
using System.Threading.Tasks;

namespace Lamp.Core.Server
{
    public interface IResponse
    {
        Task WriteAsync(string messageId, RemoteCallBackData resultMessage);
    }
}
