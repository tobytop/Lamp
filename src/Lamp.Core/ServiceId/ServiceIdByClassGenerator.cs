using Lamp.Core.Protocol.Server;
using System.Reflection;

namespace Lamp.Core.ServiceId
{
    public class ServiceIdByClassGenerator : IServiceIdGenerator
    {
        public string GenerateServiceId(MethodInfo method, string classpath, ServiceDesc desc)
        {
            var pathName = string.Empty;
            if (string.IsNullOrEmpty(classpath))
            {
                classpath = method.DeclaringType.FullName.Replace(".", "/");
            }

            var methodpath = string.Empty;
            if (string.IsNullOrEmpty(desc.RoutePath))
            {
                methodpath = "/" + method.Name;
            }
            else
            {
                methodpath = desc.RoutePath.TrimEnd('/');
            }
            if (methodpath.IndexOf('/') != 0)
            {
                methodpath = "/" + methodpath;
            }
            return classpath.TrimEnd('/').TrimStart('/') + methodpath;
        }
    }
}
