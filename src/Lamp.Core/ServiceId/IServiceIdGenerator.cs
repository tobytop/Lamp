using Lamp.Core.Protocol.Server;
using System.Reflection;

namespace Lamp.Core.ServiceId
{
    public interface IServiceIdGenerator
    {
        string GenerateServiceId(MethodInfo method,string classPath, ServiceDesc desc);
    }
}
