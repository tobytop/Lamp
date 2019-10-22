using Lamp.Core.Protocol.Server;
using System.Linq;
using System.Reflection;

namespace Lamp.Core.ServiceId
{
    public class ServiceIdGenerator : IServiceIdGenerator
    {
        public string GenerateServiceId(MethodInfo method, string classpath, ServiceDesc desc)
        {
            if (method.DeclaringType == null)
            {
                return null;
            }

            var id = $"{method.DeclaringType.FullName}.{method.Name}";
            var paras = method.GetParameters();
            if (paras.Any())
            {
                id += "(" + string.Join(",", paras.Select(i => i.Name)) + ")";
            }

            return id;
        }
    }
}
