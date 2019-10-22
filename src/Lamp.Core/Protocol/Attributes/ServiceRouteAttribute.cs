using System;

namespace Lamp.Core.Protocol.Attributes
{
    /// <summary>
    /// 服务路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ServiceRouteAttribute : Attribute
    {
        public ServiceRouteAttribute(string routeTemplate)
        {
            RouteTemplate = routeTemplate;
        }

        public string RouteTemplate { get; }
    }

    /// <summary>
    /// 服务版本
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ServiceVersionAttribute : Attribute
    {
        public ServiceVersionAttribute(string version)
        {
            Version = version;
        }

        public string Version { get; }
    }
}
